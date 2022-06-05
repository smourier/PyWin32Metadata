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
        public override string? GetInFormatChar() => "O";
        public override IEnumerable<string> DeclareParseArgTupleInputConverter() { yield return $"PyObject *ob{Parameter.Name};"; }
        public override string GetParseTupleArg() => $"&ob{Parameter.Name}";
        public override string GetInBuildValueArg() => $"ob{Parameter.Name}";
        public override IEnumerable<string> GetBuildForInterfacePostCode() { yield return $"Py_XDECREF(ob{Parameter.Name});"; }
        public override IEnumerable<string> GetBuildForInterfacePreCode() { yield return $"ob{Parameter.Name} = PyWinObject_FromULONG_PTR({GetIndirectedArgName(null, 1)});"; }
        public override IEnumerable<string> GetBuildForGatewayPostCode() { yield return $"Py_XDECREF(ob{Parameter.Name});"; }
        public override IEnumerable<string> GetInParsePostCode() { yield return $"if (bPythonIsHappy && !PyWinLong_AsULONG_PTR(ob{Parameter.Name}, (ULONG_PTR *){GetIndirectedArgName(null, 2)})) bPythonIsHappy = FALSE;"; }
    }
}
