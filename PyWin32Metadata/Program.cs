using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            // we only get interfaces
            foreach (var type in reader.TypeDefinitions.Select(reader.GetTypeDefinition).Where(t => t.Attributes.HasFlag(System.Reflection.TypeAttributes.Interface)))
            {
                var fn = reader.GetFullName(type);
                if (SkipInterface(fn))
                    continue;

                // do they have a guid attribute?
                var guid = reader.GetInteropGuid(type);
                if (!guid.HasValue)
                {
                    // these are issues in WinMD...
                    Console.WriteLine("SKIP no GUID: " + fn);
                    continue;
                }

                var baseInterfaces = new HashSet<(string, string)>();
                var bt = type.GetInterfaceImplementations();
                if (bt.Count > 0)
                {
                    foreach (var iface in bt)
                    {
                        var ifn = reader.GetFullName(iface);
                        if (ifn == Extensions.IUnknownFullName)
                            continue;

                        baseInterfaces.Add(ifn);
                    }
                }

                // make sure we at least have IUnknown
                if (baseInterfaces.Count == 0)
                {
                    baseInterfaces.Add(Extensions.IUnknownFullName);
                }

                var pi = new ParsedInterface(fn);
            }
        }

        static bool SkipInterface((string, string) fullName)
        {
            if (fullName == Extensions.IUnknownFullName)
                return true;

            return false;
        }
    }
}
