namespace PyWin32Metadata
{
    public class ArgFormatterPythonCOM : ArgFormatter
    {
        public ArgFormatterPythonCOM(GeneratorContext context, ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
        }

        public override string GetFormatChar() => "O";
        public override string DeclareParseArgTupleInputConverter() => $"PyObject *ob{Parameter.Name};";
        public override string GetParseTupleArg() => $"&ob{Parameter.Name}";
        protected override string GetPythonTypeDesc() => $"<o Py{Parameter.Type.Name}>";
        public override string GetBuildValueArg() => $"ob{Parameter.Name}";
        public override string GetBuildForInterfacePostCode() => $"Py_XDECREF(ob{Parameter.Name});";
    }
}
