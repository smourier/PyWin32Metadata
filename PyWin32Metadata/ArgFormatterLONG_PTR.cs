using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterLONG_PTR : ArgFormatter
    {
        public ArgFormatterLONG_PTR(GeneratorContext context, ParsedParameter parameter, int? builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "int/long";
        public override string? GetFormatChar() => "O";
        public override string? DeclareParseArgTupleInputConverter() => $"PyObject *ob%{Parameter.Name};";
        public override string GetParseTupleArg() => $"&ob{Parameter.Name}";
        public override string GetBuildValueArg() => $"ob{Parameter.Name}";
        public override string? GetBuildForInterfacePostCode() => $"Py_XDECREF(ob{Parameter.Name});";
        public override string? GetBuildForInterfacePreCode() => $"ob{Parameter.Name} = PyWinObject_FromULONG_PTR({GetIndirectedArgName(null, 1)});";
        public override string? GetBuildForGatewayPostCode() => $"Py_XDECREF(ob{Parameter.Name});";
        public override IEnumerable<string> GetParsePostCode() { yield return $"if (bPythonIsHappy && !PyWinLong_AsULONG_PTR(ob{Parameter.Name}, (ULONG_PTR *){GetIndirectedArgName(null, 2)})) bPythonIsHappy = FALSE;"; }
    }
}
