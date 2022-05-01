using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PyWin32Metadata
{
    public class ParsedParameter
    {
        private readonly List<ParsedCustomAttribute> _customAttributes = new();

        public ParsedParameter(ParsedMethod parent, string name, ParameterAttributes attributes, int sequenceNumber)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            Parent = parent;
            Name = name;
            Attributes = attributes;
            SequenceNumber = sequenceNumber;
        }

        public ParsedMethod Parent { get; }
        public string Name { get; }
        public ParsedType? Type { get; set; }
        public ParameterAttributes Attributes { get; }
        public int SequenceNumber { get; }
        public IList<ParsedCustomAttribute> CustomAttributes => _customAttributes;
        public bool IsConst => CustomAttributes.Any(a => a.FullName == ("Windows.Win32.Interop", "ConstAttribute"));

        public string GenerateCppMethodSignature()
        {
            var cas = IsConst ? "const" : string.Empty;
            var typeName = Type?.CppWithPointersName ?? "???";
            var list = new List<string>
            {
                cas,
                typeName,
                Name
            };
            return string.Join(" ", list.Where(i => !string.IsNullOrWhiteSpace(i)));
        }

        public override string ToString()
        {
            var cas = string.Join(string.Empty, CustomAttributes.Select(a => "[" + a.ShortName + "]"));
            var typeName = Type?.CppWithPointersName ?? "???";
            var list = new List<string>
            {
                cas,
                typeName,
                Name
            };
            return string.Join(" ", list.Where(i => !string.IsNullOrWhiteSpace(i)));
        }
    }
}
