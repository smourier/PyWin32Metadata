using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterHANDLE : ArgFormatterPythonCOM
    {
        public ArgFormatterHANDLE(GeneratorContext context, ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "<o PyHANDLE>";

        public override IEnumerable<string> GetParsePostCode() { yield return $"if (!PyWinObject_AsHANDLE(ob{Parameter.Name}, &{GetIndirectedArgName(null, 1)}, FALSE) bPythonIsHappy = FALSE;"; }
        public override IEnumerable<string> GetBuildForInterfacePreCode() { yield return $"ob{Parameter.Name} = PyWinObject_FromHANDLE({GetIndirectedArgName(null, 0)});"; }
        public override IEnumerable<string> GetInterfaceArgCleanup() { yield break; }
        public override IEnumerable<string> GetInterfaceArgCleanupGIL() { yield break; }
    }
}
