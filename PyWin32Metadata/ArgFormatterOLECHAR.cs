using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterOLECHAR : ArgFormatterPythonCOM
    {
        public ArgFormatterOLECHAR(ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "<o unicode>";

        public override IEnumerable<string> GetParsePostCode() { yield break; }
        public override string? GetInterfaceArgCleanup() => null;
        public override string? GetInterfaceArgCleanupGIL() => null;
    }
}
