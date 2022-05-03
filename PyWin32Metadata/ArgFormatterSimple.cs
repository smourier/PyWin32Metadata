using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ArgFormatterSimple : ArgFormatter
    {
        private readonly string _unconstType;
        private readonly string _pythonTypeDesc;
        private readonly string _formatChar;

        public ArgFormatterSimple(ParsedParameter parameter, string unconstType, string pythonTypeDesc, string formatChar)
            : base(parameter, 0)
        {
            _unconstType = unconstType;
            _pythonTypeDesc = pythonTypeDesc;
            _formatChar = formatChar;
        }

        protected override string GetUnconstType() => _unconstType;
        protected override string GetPythonTypeDesc() => _pythonTypeDesc;
        public override string GetFormatChar() => _formatChar;
        public override string? DeclareParseArgTupleInputConverter() => null;
        public override string? GetInterfaceArgCleanup() => null;
        public override string? GetInterfaceArgCleanupGIL() => null;
        public override IEnumerable<string> GetParsePostCode() { yield break; }
        public override string? GetBuildForGatewayPostCode() => null;
    }
}
