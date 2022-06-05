using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterIDLIST : ArgFormatterPythonCOM
    {
        public ArgFormatterIDLIST(GeneratorContext context, ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "<o PyIDL>";

        public override IEnumerable<string> GetInParsePostCode()
        {
            yield return $"if (bPythonIsHappy && !PyObject_AsPIDL(ob{Parameter.Name}, &{GetIndirectedArgName(null, 1)})) bPythonIsHappy = FALSE;";
        }

        public override IEnumerable<string> GetInterfaceArgCleanup()
        {
            yield return $"PyObject_FreePIDL(%{Parameter.Name});";
        }

        public override IEnumerable<string> GetInterfaceArgCleanupGIL() { yield break; }
        public override IEnumerable<string> GetBuildForInterfacePreCode()
        {
            yield return $"/* Review second boolean (here implicit) parameter of PyObject_FromPIDL : {Parameter.Name} */";
            yield return $"ob{Parameter.Name} = PyObject_FromPIDL({GetIndirectedArgName(null, 1)});";
        }
    }
}
