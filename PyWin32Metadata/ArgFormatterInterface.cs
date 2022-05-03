﻿using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterInterface : ArgFormatterPythonCOM
    {
        public ArgFormatterInterface(ParsedParameter parameter)
            : base(parameter, 0, 1)
        {
        }

        public override (string, string) GetInterfaceCppObjectInfo() => new(GetIndirectedArgName(1, Parameter.Type.Indirections), $"{GetUnconstType()} * {Parameter.Name}");
        public override IEnumerable<string> GetParsePostCode()
        {
            var args = GetIndirectedArgName(GatewayMode ? null : 1, 2);
            yield return $"if (bPythonIsHappy && !PyCom_InterfaceFromPyInstanceOrObject(ob{Parameter.Name}, IID_{Parameter.Type.Name}, (void **){args}, TRUE /* bNoneOK */)) bPythonIsHappy = FALSE;";
        }

        public override string GetBuildForInterfacePreCode() => $"ob{Parameter.Name} = PyCom_PyObjectFromIUnknown({Parameter.Name}, IID_{Parameter.Type.Name}, FALSE);";
        public override string GetBuildForGatewayPreCode()
        {
            var prefix = IndirectPrefix(GetDeclaredIndirections(), 1);
            return $"ob{Parameter.Name} = PyCom_PyObjectFromIUnknown({prefix}{Parameter.Name}, IID_{Parameter.Type.Name}, TRUE);";
        }

        public override string GetInterfaceArgCleanup() => $"if ({Parameter.Name}) {Parameter.Name}->Release();";
    }
}
