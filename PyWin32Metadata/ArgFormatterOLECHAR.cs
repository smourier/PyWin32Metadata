using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterOLECHAR : ArgFormatterPythonCOM
    {
        public ArgFormatterOLECHAR(GeneratorContext context, ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "<o unicode>";

        public override IEnumerable<string> GetParsePostCode() { yield break; }
        public override IEnumerable<string> GetInterfaceArgCleanup() { yield break; }
        public override IEnumerable<string> GetInterfaceArgCleanupGIL() { yield break; }
    }
}
