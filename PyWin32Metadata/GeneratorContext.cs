using System.Collections.Generic;
using System.Reflection.Metadata;

namespace PyWin32Metadata
{
    public class GeneratorContext
    {
        private readonly Dictionary<(string, string), ParsedInterface> _interfaces = new();
        private readonly Dictionary<(string, string), ParsedStructure> _structures = new();
        private readonly HashSet<(string, string)> _handles = new();
        private readonly HashSet<(string, string)> _functions = new();

        public GeneratorContext(MetadataReader reader)
        {
            Reader = reader;
        }

        public MetadataReader Reader { get; }
        public IDictionary<(string, string), ParsedInterface> Interfaces => _interfaces;
        public IDictionary<(string, string), ParsedStructure> Structures => _structures;
        public ISet<(string, string)> Handles => _handles;
        public ISet<(string, string)> Functions => _functions;
    }
}
