using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterSimple : ArgFormatter
    {
        private readonly string _unconstType;
        private readonly string _pythonTypeDesc;
        private readonly string _formatChar;

        public ArgFormatterSimple(GeneratorContext context, ParsedParameter parameter, string unconstType, string pythonTypeDesc, string formatChar)
            : base(context, parameter, 0)
        {
            _unconstType = unconstType;
            _pythonTypeDesc = pythonTypeDesc;
            _formatChar = formatChar;
        }

        protected override string GetUnconstType() => _unconstType;
        protected override string GetPythonTypeDesc() => _pythonTypeDesc;
        public override string GetFormatChar() => _formatChar;
        public override IEnumerable<string> DeclareParseArgTupleInputConverter() { yield break; }
        public override IEnumerable<string> GetInterfaceArgCleanup() { yield break; }
        public override IEnumerable<string> GetInterfaceArgCleanupGIL() { yield break; }
        public override IEnumerable<string> GetParsePostCode() { yield break; }
        public override IEnumerable<string> GetBuildForGatewayPostCode() { yield break; }
        public override IEnumerable<string> GetBuildForInterfacePreCode() { yield break; }
        public override IEnumerable<string> GetBuildForInterfacePostCode() { yield break; }
    }
}
