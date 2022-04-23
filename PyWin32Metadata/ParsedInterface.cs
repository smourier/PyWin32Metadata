namespace PyWin32Metadata
{
    public class ParsedInterface
    {
        public ParsedInterface((string, string) fullName)
        {
            FullName = fullName;
        }

        public (string, string) FullName { get; }
        public string Name => FullName.Item2;
        public string Namespace => FullName.Item1;
        public string FullNameString => Namespace + "." + Name;

        public override string ToString() => FullNameString;
    }
}
