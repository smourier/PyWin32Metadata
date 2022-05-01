using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace PyWin32Metadata
{
    class Program
    {
        static void Main()
        {
            // Win32Metadata.WinMdPath should be automatically generated before build
            var winmd = Path.Combine(Win32Metadata.WinMdPath, "Windows.Win32.winmd");
            using var stream = File.OpenRead(winmd);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();

            var interfaces = new Dictionary<(string, string), ParsedInterface>();

            var count = 0;
            // we only get interfaces
            foreach (var type in reader.TypeDefinitions.Select(reader.GetTypeDefinition).Where(t => t.Attributes.HasFlag(TypeAttributes.Interface)))
            {
                var fn = reader.GetFullName(type);

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
                        if (ifn == ParsedInterface.IUnknownFullName)
                            continue;

                        baseInterfaces.Add(ifn);
                    }
                }

                // make sure we at least have IUnknown
                if (baseInterfaces.Count == 0)
                {
                    baseInterfaces.Add(ParsedInterface.IUnknownFullName);
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

                    var dec = method.DecodeSignature(new SignatureTypeProvider(), null);
                    pm.ReturnType = dec.ReturnType;
                    if (dec.ParameterTypes.Length != pm.Parameters.Count)
                        throw new InvalidOperationException();

                    for (var i = 0; i < dec.ParameterTypes.Length; i++)
                    {
                        pm.Parameters[i].Type = dec.ParameterTypes[i];
                    }
                }

                interfaces.Add(pi.FullName, pi);
                count++;
            }

            // resolve base interfaces & interfaces as parameter
            foreach (var pi in interfaces)
            {
                pi.Value.BaseInterface = interfaces[pi.Value.BaseFullName];
                foreach (var m in pi.Value.Methods)
                {
                    foreach (var p in m.Parameters)
                    {
                        if (p.Type != null && interfaces.TryGetValue(p.Type.FullName, out var piface))
                        {
                            // a .NET interface is not marked as of pointer type, but it is, for C++
                            var refType = new ParsedType(piface.FullName) { Pointers = p.Type.Pointers + 1 };
                            p.Type = refType;
                        }
                    }
                }
            }

            foreach (var pi in interfaces)
            {
                if (pi.Value.Name == "IAntimalwareProvider")
                {
                }
                Console.WriteLine(pi.Value.GenerateCppDeclaration());
            }
            return;

            DumpShell("IShellItem");
            DumpShell("IShellItem2");
            DumpShell("IShellView");

            void DumpShell(string n) => Dump("Windows.Win32.UI.Shell", n);
            void Dump(string ns, string n)
            {
                var ifa = interfaces[(ns, n)];
                Console.WriteLine(ifa.GenerateCppDeclaration());
            }
        }
    }
}
