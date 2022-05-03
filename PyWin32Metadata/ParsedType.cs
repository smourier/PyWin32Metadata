using System;
using System.Reflection.Metadata;

namespace PyWin32Metadata
{
    public class ParsedType : IEquatable<ParsedType>
    {
        public static readonly (string, string) IUnknownFullName = ("Windows.Win32.System.Com", "IUnknown");
        public static readonly (string, string) IDispatchFullName = ("Windows.Win32.System.Com", "IDispatch");
        public static readonly (string, string) HRESULTFullName = ("Windows.Win32.Foundation", "HRESULT");

        public ParsedType(Type type)

            : this((type.Namespace ?? string.Empty, type.Name))
        {
        }

        public ParsedType((string, string) fullName)
        {
            FullName = fullName;
        }

        public (string, string) FullName { get; }
        public int Indirections { get; set; }
        public ArrayShape? ArrayShape { get; set; }
        public string Name => FullName.Item2;
        public string Namespace => FullName.Item1;
        public string FullNameString => Namespace + "." + Name;
        public string WithIndirectionsName => Name + new string('*', Indirections);
        public string CppWithIndirectionsName => CppName + new string('*', Indirections) + ArrayText;
        public ParsedType PointerType => new ParsedType(FullName) { Indirections = Indirections + 1 };
        public bool IsUnknown => FullName == IUnknownFullName;
        public bool IsDispatch => FullName == IDispatchFullName;
        public bool IsHRESULT => FullName == HRESULTFullName;

        public ParsedType Clone()
        {
            var clone = new ParsedType(FullName);
            clone.ArrayShape = ArrayShape;
            clone.Indirections = Indirections;
            return clone;
        }

        private string? ArrayText
        {
            get
            {
                if (!ArrayShape.HasValue)
                    return null;

                string? s = null;
                for (var i = 0; i < ArrayShape.Value.Rank; i++)
                {
                    if (ArrayShape.Value.LowerBounds[i] != 0)
                        throw new InvalidOperationException();

                    s += "[" + ArrayShape.Value.Sizes + "]";
                }
                return s;
            }
        }

        public string CppName
        {
            get
            {
                if (FullNameString == typeof(void).FullName)
                    return "void";

                if (FullNameString == typeof(Guid).FullName)
                    return "GUID";

                if (FullNameString == typeof(int).FullName)
                    return "int";

                if (FullNameString == typeof(uint).FullName)
                    return "unsigned int";

                if (FullNameString == typeof(short).FullName)
                    return "short";

                if (FullNameString == typeof(ushort).FullName)
                    return "unsigned short";

                if (FullNameString == typeof(long).FullName)
                    return "LONGLONG";

                if (FullNameString == typeof(ulong).FullName)
                    return "ULONGLONG";

                if (FullNameString == typeof(bool).FullName)
                    return "BOOL";

                if (FullNameString == typeof(byte).FullName)
                    return "unsigned char";

                if (FullNameString == typeof(sbyte).FullName)
                    return "char";

                if (FullNameString == typeof(float).FullName)
                    return "float";

                if (FullNameString == typeof(double).FullName)
                    return "double";

                if (FullNameString == typeof(UIntPtr).FullName)
                    return "UINT_PTR";

                if (FullNameString == typeof(IntPtr).FullName)
                    return "INT_PTR";

                return Name;
            }
        }

        public override int GetHashCode() => FullName.GetHashCode();
        public override bool Equals(object? obj) => Equals(obj as ParsedType);
        public bool Equals(ParsedType? other) => other != null && other.FullName == FullName && other.Indirections == Indirections;
        public override string ToString() => WithIndirectionsName;
    }
}
