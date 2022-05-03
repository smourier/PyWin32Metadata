using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ParsedStructure : ParsedType
    {
        private readonly List<ParsedField> _fields = new();

        public ParsedStructure((string, string) fullName)
            : base(fullName)
        {
        }

        public IList<ParsedField> Fields => _fields;
    }
}
