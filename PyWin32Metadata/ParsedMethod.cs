using System;
using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class ParsedMethod
    {
        private readonly List<ParsedParameter> _parameters = new();

        public ParsedMethod(ParsedInterface parent, string name)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            Parent = parent;
            Name = name;
        }

        public ParsedInterface Parent { get; }
        public string Name { get; }
        public ParsedType? ReturnType { get; set; }
        public IList<ParsedParameter> Parameters => _parameters;

        public override string ToString() => Name;
    }
}
