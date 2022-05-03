using System.Collections.Generic;

namespace PyWin32Metadata
{
    public class GeneratorContext
    {
        private readonly Dictionary<(string, string), ParsedInterface> _interfaces = new();

        public IDictionary<(string, string), ParsedInterface> Interfaces => _interfaces;
    }
}
