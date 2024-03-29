﻿namespace PyWin32Metadata
{
    public class ParsedCustomAttribute : ParsedType
    {
        public ParsedCustomAttribute((string, string) fullName)
            : base(fullName)
        {
        }

        public string ShortName
        {
            get
            {
                const string token = "Attribute";
                if (Name == null)
                    return string.Empty;

                if (Name.Length > token.Length && Name.EndsWith(token))
                    return Name.Substring(0, Name.Length - token.Length);

                return Name;
            }
        }

        public override string ToString() => ShortName;
    }
}
