using System;

namespace PyWin32Metadata
{
    public class ParsedField
    {
        public ParsedField(ParsedStructure parent, string name, ParsedType type)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            Parent = parent;
            Name = name;
            Type = type;
        }

        public ParsedStructure Parent { get; }
        public string Name { get; }
        public ParsedType Type { get; }

        public override string ToString()
        {
            var typeName = Type?.CppWithIndirectionsName ?? "???";
            return typeName + " " + Name;
        }
    }
}
