using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterInterface : ArgFormatterPythonCOM
    {
        public ArgFormatterInterface(GeneratorContext context, ParsedParameter parameter)
            : base(context, parameter, 0, 1)
        {
        }

        private string IIDName
        {
            get
            {
                if (Parameter.Type.FullName == ("System", "Void"))
                    return "IUnknown";

                return Parameter.Type.Name;
            }
        }

        public override (string, string) GetInterfaceCppObjectInfo() => new(GetIndirectedArgName(1, Parameter.Type.Indirections), $"{GetUnconstType()} * {Parameter.Name}");
        public override IEnumerable<string> GetInParsePostCode()
        {
            var args = GetIndirectedArgName(GatewayMode ? null : 1, 2);
            yield return $"if (bPythonIsHappy && !PyCom_InterfaceFromPyInstanceOrObject(ob{Parameter.Name}, IID_{IIDName}, (void **){args}, TRUE /* bNoneOK */)) bPythonIsHappy = FALSE;";
        }

        public override IEnumerable<string> GetBuildForInterfacePreCode() { yield return $"ob{Parameter.Name} = PyCom_PyObjectFromIUnknown((IUnknown*){Parameter.Name}, IID_{IIDName}, FALSE);"; }
        public override IEnumerable<string> GetBuildForGatewayPreCode()
        {
            var prefix = IndirectPrefix(GetDeclaredIndirections(), 1);
            yield return $"ob{Parameter.Name} = PyCom_PyObjectFromIUnknown((IUnknown*){prefix}{Parameter.Name}, IID_{IIDName}, TRUE);";
        }

        public override IEnumerable<string> GetInterfaceArgCleanup()
        {
            yield return $"if ({Parameter.Name}) {Parameter.Name}->Release();";
        }
    }
}
