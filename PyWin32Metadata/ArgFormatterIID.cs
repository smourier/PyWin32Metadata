using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterIID : ArgFormatterPythonCOM
    {
        public ArgFormatterIID(GeneratorContext context, ParsedParameter parameter, int builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
        }

        protected override string GetPythonTypeDesc() => "<o PyIID>";

        public override IEnumerable<string> GetParsePostCode() { yield return $"if (!PyWinObject_AsIID(ob{Parameter.Name}, &{Parameter.Name})) bPythonIsHappy = FALSE;"; }
        public override string? GetBuildForInterfacePreCode() => $"ob{Parameter.Name} = PyWinObject_FromIID({GetIndirectedArgName(null, 0)});";
        public override (string, string) GetInterfaceCppObjectInfo() => new(Parameter.Name, $"IID {Parameter.Name}");
    }
}
