﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace PyWin32Metadata
{
    static class Program
    {
        static void Main()
        {
            // Win32Metadata.WinMdPath should be automatically generated before build
            var winmd = Path.Combine(Win32Metadata.WinMdPath, "Windows.Win32.winmd");
            using var stream = File.OpenRead(winmd);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();

            var ctx = new GeneratorContext(reader);
            var count = 0;
            foreach (var type in reader.TypeDefinitions.Select(reader.GetTypeDefinition))
            {
                var fn = reader.GetFullName(type);

                // HFONT, etc.
                if (reader.IsHandle(type))
                {
                    ctx.Handles.Add(fn);
                    continue;
                }

                // LPFNXX, etc.
                if (reader.IsFunction(type))
                {
                    ctx.Functions.Add(fn);
                    continue;
                }

                // structs
                if (reader.IsStructure(type))
                {
                    var ps = new ParsedStructure(fn);
                    ctx.Structures[ps.FullName] = ps;

                    var fields = type.GetFields();
                    foreach (var f in fields)
                    {
                        var field = reader.GetFieldDefinition(f);
                        var pf = new ParsedField(ps, reader.GetString(field.Name), field.DecodeSignature(SignatureTypeProvider.Instance, null));
                        ps.Fields.Add(pf);
                    }
                    continue;
                }

                if (!type.Attributes.HasFlag(TypeAttributes.Interface))
                    continue;

                // do they have a guid attribute?
                var guid = reader.GetInteropGuid(type);
                if (!guid.HasValue)
                {
                    // these are issues in WinMD...
                    //Console.WriteLine("SKIP no GUID: " + fn);
                    continue;
                }

                var baseInterfaces = new HashSet<(string, string)>();
                var bt = type.GetInterfaceImplementations();
                if (bt.Count > 0)
                {
                    foreach (var iface in bt)
                    {
                        var ifn = reader.GetFullName(iface);
                        if (ifn == ParsedType.IUnknownFullName)
                            continue;

                        baseInterfaces.Add(ifn);
                    }
                }

                // make sure we at least have IUnknown
                if (baseInterfaces.Count == 0)
                {
                    baseInterfaces.Add(ParsedType.IUnknownFullName);
                }
                else if (baseInterfaces.Count > 1)
                {
                    Console.WriteLine("SKIP multiple interfaces: " + fn);
                    continue;
                }

                var pi = new ParsedInterface(fn, baseInterfaces.First());
                foreach (var methodHandle in type.GetMethods())
                {
                    var method = reader.GetMethodDefinition(methodHandle);
                    var pm = new ParsedMethod(pi, reader.GetString(method.Name));
                    pi.Methods.Add(pm);

                    foreach (var parameterHandle in method.GetParameters())
                    {
                        var parameter = reader.GetParameter(parameterHandle);
                        var pp = new ParsedParameter(pm,
                            reader.GetString(parameter.Name),
                            parameter.Attributes,
                            parameter.SequenceNumber);

                        // 'this' ?
                        if (string.IsNullOrEmpty(pp.Name) && pp.SequenceNumber == 0)
                            continue;

                        pm.Parameters.Add(pp);

                        foreach (var ca in parameter.GetCustomAttributes())
                        {
                            pp.CustomAttributes.Add(new ParsedCustomAttribute(reader.GetFullName(ca)));
                        }
                    }

                    var dec = method.DecodeSignature(SignatureTypeProvider.Instance, null);
                    pm.ReturnType = dec.ReturnType;
                    if (dec.ParameterTypes.Length != pm.Parameters.Count)
                        throw new InvalidOperationException();

                    for (var i = 0; i < dec.ParameterTypes.Length; i++)
                    {
                        pm.Parameters[i].Type = dec.ParameterTypes[i];
                    }
                }

                ctx.Interfaces.Add(pi.FullName, pi);
                count++;
            }

            // resolve base interfaces & interfaces as parameter
            foreach (var pi in ctx.Interfaces)
            {
                pi.Value.BaseInterface = ctx.Interfaces[pi.Value.BaseFullName];
                foreach (var m in pi.Value.Methods)
                {
                    foreach (var p in m.Parameters)
                    {
                        if (p.Type != null && ctx.Interfaces.TryGetValue(p.Type.FullName, out var piface))
                        {
                            // a .NET interface is not marked as of pointer type, but it is, for C++
                            var refType = new ParsedType(piface.FullName) { Indirections = p.Type.Indirections + 1 };
                            p.Type = refType;
                        }
                    }
                }
            }

            var path = Path.GetFullPath("Generated");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var pi in ctx.Interfaces)
            {
                if (!pi.Value.Name.StartsWith("IFolderView"))
                    continue;

                //Console.WriteLine(pi.Value.Name);
                File.WriteAllText(Path.Combine(path, "Py" + pi.Value.Name + ".h"), pi.Value.GenerateCppDeclaration(ctx));
                File.WriteAllText(Path.Combine(path, "Py" + pi.Value.Name + ".cpp"), pi.Value.GenerateCppImplementation(ctx));
                //Console.WriteLine(pi.Value.GenerateCppDeclaration());
                //Console.WriteLine(pi.Value.GenerateCppImplementation());
            }
            return;

            DumpShell("IShellItem");
            DumpShell("IShellItem2");
            DumpShell("IShellView");

            void DumpShell(string n) => Dump("Windows.Win32.UI.Shell", n);
            void Dump(string ns, string n)
            {
                var ifa = ctx.Interfaces[(ns, n)];
                Console.WriteLine(ifa.GenerateCppDeclaration(ctx));
                Console.WriteLine(ifa.GenerateCppImplementation(ctx));
            }
        }
    }
}
