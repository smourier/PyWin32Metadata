﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PyWin32Metadata
{
    public abstract class ArgFormatter
    {
        internal static Dictionary<string, (string, string, string)> _convertSimpleTypes = new()
        {
            { "BOOL", ("BOOL", "int", "i") },
            { "UINT", ("UINT", "int", "i") },
            { "BYTE", ("BYTE", "int", "i") },
            { "INT", ("INT", "int", "i") },
            { "DWORD", ("DWORD", "int", "l") },
            { "HRESULT", ("HRESULT", "int", "l") },
            { "ULONG", ("ULONG", "int", "l") },
            { "LONG", ("LONG", "int", "l") },
            { "int", ("int", "int", "i") },
            { "long", ("long", "int", "l") },
            { "DISPID", ("DISPID", "long", "l") },
            { "APPBREAKFLAGS", ("int", "int", "i") },
            { "BREAKRESUMEACTION", ("int", "int", "i") },
            { "ERRORRESUMEACTION", ("int", "int", "i") },
            { "BREAKREASON", ("int", "int", "i") },
            { "BREAKPOINT_STATE", ("int", "int", "i") },
            { "BREAKRESUME_ACTION", ("int", "int", "i") },
            { "SOURCE_TEXT_ATTR", ("int", "int", "i") },
            { "TEXT_DOC_ATTR", ("int", "int", "i") },
            { "QUERYOPTION", ("int", "int", "i") },
            { "PARSEACTION", ("int", "int", "i") },
            // .NET
            { "Int32", ("INT", "int", "i") },
            { "UInt32", ("UINT", "int", "i") },
        };

        internal static Dictionary<string, (Type, int, int)> _allConverters = new()
        {
            { "const OLECHAR", (typeof(ArgFormatterOLECHAR), 0, 1) },
            { "WCHAR", (typeof(ArgFormatterOLECHAR), 0, 1) },
            { "OLECHAR", (typeof(ArgFormatterOLECHAR), 0, 1) },
            { "LPCOLESTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "PCOLESTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPOLESTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "POLESTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPCWSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "PCWSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPWSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "PWSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPCSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "PCSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },

            { "HANDLE", (typeof(ArgFormatterHANDLE), 0, 0) },
            { "ITEMIDLIST", (typeof(ArgFormatterIDLIST), 0, 0) },

            { "CLSID", (typeof(ArgFormatterIID), 0, 0) },
            { "IID", (typeof(ArgFormatterIID), 0, 0) },
            { "GUID", (typeof(ArgFormatterIID), 0, 0) },
            { "const GUID", (typeof(ArgFormatterIID), 0, 0) },
            { "const IID", (typeof(ArgFormatterIID), 0, 0) },
            { "REFCLSID", (typeof(ArgFormatterIID), 0, 0) },
            { "REFIID", (typeof(ArgFormatterIID), 0, 0) },
            { "REFGUID", (typeof(ArgFormatterIID), 0, 0) },


            { "HWND", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "HMENU", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "HOLEMENU", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "HICON", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "HDC", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "LPARAM", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "WPARAM", (typeof(ArgFormatterLONG_PTR), 1, 0) },
            { "LRESULT", (typeof(ArgFormatterLONG_PTR), 1, 0) },

            // .NET
            { "Guid", (typeof(ArgFormatterIID), 0, 0) },
        };

        protected static string IndirectPrefix(int indirectionFrom, int indirectionTo)
        {
            var diff = indirectionFrom - indirectionTo;
            if (diff == 0)
                return string.Empty;

            if (diff == -1)
                return "&";

            if (diff == 1)
                return "*";

            return "/* IndirectPrefix diff " + diff + " is not supported.*/";
        }

        private readonly static HashSet<string> _notFound = new();

        public static ArgFormatter? GetArgConverter(GeneratorContext context, ParsedParameter parameter)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter.Type == null)
                throw new InvalidOperationException();

            if (context.Interfaces.ContainsKey(parameter.Type.FullName))
                return new ArgFormatterInterface(context, parameter);

            if (_convertSimpleTypes.TryGetValue(parameter.Type.Name, out var converter))
                return new ArgFormatterSimple(context, parameter, converter.Item1, converter.Item2, converter.Item3);

            if (_allConverters.TryGetValue(parameter.Type.Name, out var converter2))
            {
                var args = new List<object>
                {
                    context,
                    parameter,
                    converter2.Item2,
                    converter2.Item3
                };
                return Activator.CreateInstance(converter2.Item1, args.ToArray()) as ArgFormatter;
            }

            if (context.Handles.Contains(parameter.Type.FullName))
                return new ArgFormatterLONG_PTR(context, parameter, 1, 0);

            if (context.Functions.Contains(parameter.Type.FullName))
                return new ArgFormatterLONG_PTR(context, parameter, 1, 0);

            if (parameter.Type.FullName == ("System", "Void"))
            {
                if (parameter.Type.Indirections == 1)
                    return new ArgFormatterLONG_PTR(context, parameter, 1, 0);

                if (parameter.Type.Indirections == 2)
                    return new ArgFormatterInterface(context, parameter);
            }

            if (context.Structures.TryGetValue(parameter.Type.FullName, out var ps))
                return new ArgFormatterStruct(context, parameter, ps, null);

            if (!_notFound.Contains(parameter.Type.FullNameString))
            {
                //var type = context.Reader.TypeDefinitions.FirstOrDefault(h => context.Reader.GetFullName(h) == parameter.Type.FullName);
                //if (type.IsNil)
                {
                    _notFound.Add(parameter.Type.FullNameString);
                    Console.WriteLine("Not Found: " + parameter.Type.FullNameString);
                }

                //var fn = context.Reader.GetFullName(type);
            }
            return null;
        }

        protected ArgFormatter(GeneratorContext context, ParsedParameter parameter, int? builtinIndirection, int declaredIndirection = 0)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            Context = context;
            Parameter = parameter;
            BuiltinIndirection = builtinIndirection;
            DeclaredIndirection = declaredIndirection;
        }

        public GeneratorContext Context { get; }
        public ParsedParameter Parameter { get; }
        public int? BuiltinIndirection { get; }
        public int DeclaredIndirection { get; }
        public bool GatewayMode { get; set; }

        protected int GetDeclaredIndirections()
        {
            if (Parameter.Type == null)
                throw new InvalidOperationException();

            return Parameter.Type.Indirections;
        }

        protected virtual string GetUnconstType() => Parameter.Type.GetCppWithIndirectionsName(Parameter);
        protected virtual string? GetPythonTypeDesc() => null;
        public virtual string? GetInFormatChar() => null;
        public virtual string? GetOutFormatChar() => GetInFormatChar();

        public virtual string GetIndirectedArgName(int? indirectFrom, int indirectionTo)
        {
            if (!indirectFrom.HasValue)
            {
                indirectFrom = GetDeclaredIndirections() + BuiltinIndirection;
            }

            return IndirectPrefix(indirectFrom.GetValueOrDefault(), indirectionTo) + Parameter.Name;
        }

        public virtual string GetInBuildValueArg() => Parameter.Name;
        public virtual string GetOutBuildValueArg() => GetInBuildValueArg();

        public virtual string GetParseTupleArg()
        {
            if (GatewayMode)
                return GetIndirectedArgName(null, 1);

            return GetIndirectedArgName(BuiltinIndirection, 1);
        }

        public virtual (string, string) GetInterfaceCppObjectInfo() => new(GetIndirectedArgName(BuiltinIndirection, Parameter.Type.Indirections + BuiltinIndirection.GetValueOrDefault()), $"{GetUnconstType()} {Parameter.Name}");

        public virtual IEnumerable<string> GetInterfaceArgCleanup()
        {
            yield return $"/* GetInterfaceArgCleanup output goes here: {Parameter.Name} */";
        }

        public virtual IEnumerable<string> GetInterfaceArgCleanupGIL()
        {
            yield return $"/* GetInterfaceArgCleanup (GIL held) output goes here: {Parameter.Name} */";
        }

        public virtual IEnumerable<string> DeclareParseArgTupleInputConverter()
        {
            yield return $"/* Declare ParseArgTupleInputConverter goes here: {Parameter.Name} */";
        }

        public virtual IEnumerable<string> GetInParsePostCode()
        {
            yield return $"/* GetInParsePostCode code goes here: {Parameter.Name} */";
        }

        public virtual IEnumerable<string> GetOutParsePostCode()
        {
            var warning = GetBuildForInterfacePreCode().ToArray();
            if (warning.Length > 0 && warning[0].StartsWith("/* GetInParsePostCode code goes here:"))
            {
                yield return $"/* GetOutParsePostCode code goes here: {Parameter.Name} */";
            }
            else
            {
                foreach (var code in warning)
                {
                    yield return code;
                }
            }
        }

        public virtual IEnumerable<string> GetBuildForInterfacePreCode()
        {
            yield return $"/* GetBuildForInterfacePreCode goes here: {Parameter.Name} */";
        }

        public virtual IEnumerable<string> GetBuildForGatewayPreCode()
        {
            var warning = GetBuildForInterfacePreCode().ToArray();
            if (warning.Length > 0 && warning[0].StartsWith("/* GetBuildForInterfacePreCode goes here:"))
            {
                yield return $"/* GetBuildForGatewayPreCode goes here: {Parameter.Name} */";
            }
            else
            {
                foreach (var code in warning)
                {
                    yield return code;
                }
            }
        }

        public virtual IEnumerable<string> GetBuildForInterfacePostCode()
        {
            yield return $"/* GetBuildForInterfacePostCode goes here: {Parameter.Name} */";
        }

        public virtual IEnumerable<string> GetBuildForGatewayPostCode()
        {
            var warning = GetBuildForInterfacePostCode().ToArray();
            if (warning.Length > 0 && warning[0].StartsWith("/* GetBuildForInterfacePostCode goes here:"))
            {
                yield return $"/* GetBuildForGatewayPostCode goes here: {Parameter.Name} */";
            }
            else
            {
                foreach (var code in warning)
                {
                    yield return code;
                }
            }
        }

        public virtual string GetAutoduckString() => $"// @pyparm {GetPythonTypeDesc()}|{Parameter.Name}||Description for {Parameter.Name}";
    }
}
