using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PyWin32Metadata
{
    public class ParsedParameter
    {
        private readonly List<ParsedCustomAttribute> _customAttributes = new();

        public ParsedParameter(ParsedMethod? parent, string name, ParameterAttributes attributes, int sequenceNumber)
        {
            Parent = parent; // can be null
            Name = name;
            Attributes = attributes;
            SequenceNumber = sequenceNumber;
            Type = new ParsedType(("???", "???")); // will be setup after
        }

        public ParsedMethod? Parent { get; }
        public string Name { get; }
        public ParsedType Type { get; set; }
        public ParameterAttributes Attributes { get; }
        public int SequenceNumber { get; }
        public IList<ParsedCustomAttribute> CustomAttributes => _customAttributes;
        public bool IsConst => CustomAttributes.Any(a => a.FullName == ("Windows.Win32.Interop", "ConstAttribute"));
        public bool IsOut => Attributes.HasFlag(ParameterAttributes.Out);
        public bool IsIn => Attributes.HasFlag(ParameterAttributes.In);
        public bool IsOptional => Attributes.HasFlag(ParameterAttributes.Optional);

        public string GenerateCppMethodSignature()
        {
            if (IsConst && Type?.GetCppWithIndirectionsName(this) == "GUID*")
                return "const IID &";

            var cas = IsConst ? "const" : string.Empty;
            var typeName = Type?.GetCppWithIndirectionsName(this) ?? "???";
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
            var typeName = Type?.GetCppWithIndirectionsName(this) ?? "???";
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
