namespace PyWin32Metadata
{
    public class ArgFormatterOLECHAR : ArgFormatterPythonCOM
    {
        public ArgFormatterOLECHAR(ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "<o unicode>";
    }
}
