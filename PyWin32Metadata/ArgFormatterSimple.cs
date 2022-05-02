namespace PyWin32Metadata
{
    public class ArgFormatterSimple : ArgFormatter
    {
        private readonly string _pythonTypeDesc;
        private readonly string _formatChar;

        public ArgFormatterSimple(ParsedParameter parameter, string pythonTypeDesc, string formatChar)
            : base(parameter, 0)
        {
            _pythonTypeDesc = pythonTypeDesc;
            _formatChar = formatChar;
        }

        public override string GetFormatChar() => _formatChar;
        protected override string GetPythonTypeDesc() => _pythonTypeDesc;
    }
}
