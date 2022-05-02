using System;
using System.Collections.Generic;

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
        };

        internal static Dictionary<string, (Type, int, int)> _allConverters = new()
        {
            { "const OLECHAR", (typeof(ArgFormatterOLECHAR), 0, 1) },
            { "WCHAR", (typeof(ArgFormatterOLECHAR), 0, 1) },
            { "OLECHAR", (typeof(ArgFormatterOLECHAR), 0, 1) },
            { "LPCOLESTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPOLESTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPCWSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPWSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
            { "LPCSTR", (typeof(ArgFormatterOLECHAR), 1, 1) },
        };

        public static string IndirectPrefix(int indirectionFrom, int indirectionTo)
        {
            var dif = indirectionFrom - indirectionTo;
            if (dif == 0)
                return string.Empty;

            if (dif == -1)
                return "&";

            if (dif == 1)
                return "*";

            throw new NotSupportedException();
        }

        public static ArgFormatter? GetArgConverter(ParsedParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter.Type == null)
                throw new InvalidOperationException();

            if (_convertSimpleTypes.TryGetValue(parameter.Type.Name, out var converter))
                return new ArgFormatterSimple(parameter, converter.Item2, converter.Item3);

            if (_allConverters.TryGetValue(parameter.Type.Name, out var converter2))
            {
                var args = new List<object>
                {
                    parameter,
                    converter2.Item2,
                    converter2.Item3
                };
                return Activator.CreateInstance(converter2.Item1, args.ToArray()) as ArgFormatter;
            }

            return null;
        }

        protected ArgFormatter(ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            Parameter = parameter;
            BuiltinIndirection = builtinIndirection;
            DeclaredIndirection = declaredIndirection;
        }

        public ParsedParameter Parameter { get; }
        public int BuiltinIndirection { get; }
        public int DeclaredIndirection { get; }
        public bool GatewayMode { get; set; }

        protected int GetDeclaredIndirections()
        {
            if (Parameter.Type == null)
                throw new InvalidOperationException();

            return Parameter.Type.Indirections;
        }

        protected virtual string GetPythonTypeDesc() => null;
        public virtual string GetFormatChar() => null;

        public virtual string GetIndirectedArgName(int? indirectFrom, int indirectionTo)
        {
            if (!indirectFrom.HasValue)
            {
                indirectFrom = GetDeclaredIndirections() + BuiltinIndirection;
            }

            return IndirectPrefix(indirectFrom.Value, indirectionTo) + Parameter.Name;
        }

        public virtual string GetBuildValueArg() => Parameter.Name;

        public virtual string GetParseTupleArg()
        {
            if (GatewayMode)
                return GetIndirectedArgName(null, 1);

            return GetIndirectedArgName(BuiltinIndirection, 1);
        }

        public virtual (string, string) GetInterfaceCppObjectInfo() => new(GetIndirectedArgName(BuiltinIndirection, Parameter.Type.Indirections + BuiltinIndirection), $"{GetUnconstType()} {Parameter.Name}");

        public virtual string GetInterfaceArgCleanup() => $"/* GetInterfaceArgCleanup output goes here: {Parameter.Name} */";
        public virtual string GetInterfaceArgCleanupGIL() => $"/* GetInterfaceArgCleanup (GIL held) output goes here: {Parameter.Name} */";
        public virtual string GetUnconstType() => Parameter.Type.FullNameString;
        public virtual string DeclareParseArgTupleInputConverter() => $"/* Declare ParseArgTupleInputConverter goes here: {Parameter.Name} */";
        public virtual string GetParsePostCode() => $"/* GetParsePostCode code goes here: {Parameter.Name} */";
        public virtual string GetBuildForInterfacePreCode() => $"/* GetBuildForInterfacePreCode goes here: {Parameter.Name} */";
        public virtual string GetBuildForGatewayPreCode()
        {
            var s = GetBuildForInterfacePreCode();
            if (s.Substring(0, 4) == "/* G")
                return $"/* GetBuildForGatewayPreCode goes here: {Parameter.Name} */";

            return s;
        }

        public virtual string GetBuildForInterfacePostCode() => $"/* GetBuildForInterfacePostCode goes here: {Parameter.Name} */";
        public virtual string GetBuildForGatewayPostCode()
        {
            var s = GetBuildForInterfacePostCode();
            if (s.Substring(0, 4) == "/* G")
                return $"/* GetBuildForGatewayPostCode goes here: {Parameter.Name} */";

            return s;
        }

        public virtual string GetAutoduckString() => $"// @pyparm {GetPythonTypeDesc()}|{Parameter.Name}||Description for {Parameter.Name}";
    }
}
