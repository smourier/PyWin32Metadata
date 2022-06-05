using System;
using System.Collections.Generic;
using System.Reflection;

namespace PyWin32Metadata
{
    public class ArgFormatterStruct : ArgFormatter
    {
        public ArgFormatterStruct(GeneratorContext context, ParsedParameter parameter, ParsedStructure structure, int? builtinIndirection, int declaredIndirection = 0)
            : base(context, parameter, builtinIndirection, declaredIndirection)
        {
            Structure = structure;
        }

        public ParsedStructure Structure { get; }
        public override string GetInFormatChar() => "O";
        public override IEnumerable<string> DeclareParseArgTupleInputConverter() { yield return $"PyObject *ob{Parameter.Name};"; }
        public override string GetParseTupleArg() => $"&ob{Parameter.Name}";
        public override IEnumerable<string> GetInParsePostCode()
        {
            if (Structure.Fields.Count == 1)
                yield return $"if (bPythonIsHappy && !PyWinObject_As{Parameter.Type.Name}(ob{Parameter.Name}, &{GetIndirectedArgName(null, 0)}, FALSE) bPythonIsHappy = FALSE;";

            var format = new List<string>();
            var args = new List<string>();

            BuildParsePostCode(Context, this, format, args);
            yield return $"if (bPythonIsHappy && !PyArg_ParseTuple(ob{Parameter.Name}, \"{string.Join(string.Empty, format)}\", {string.Join(", ", args)}) bPythonIsHappy = FALSE;";
        }

        private static void BuildParsePostCode(GeneratorContext context, ArgFormatterStruct structFormatter, List<string> format, List<string> args)
        {
            foreach (var field in structFormatter.Structure.Fields)
            {
                var pp = new ParsedParameter(null, field.Name, ParameterAttributes.None, 0)
                {
                    Type = field.Type.Clone()
                };

                string? formatChar;
                // is it sub structure?
                if (field.Type.Indirections == 0 && context.Structures.TryGetValue(field.Type.FullName, out var ps))
                {
                    var subFormatter = new ArgFormatterStruct(context, structFormatter.Parameter, ps, null);
                    var subFormat = new List<string>();
                    var subArgs = new List<string>();
                    BuildParsePostCode(context, subFormatter, subFormat, subArgs);
                    formatChar = "(" + string.Join(string.Empty, subFormat) + ")";
                }
                else
                {
                    var formatter = GetArgConverter(context, pp);
                    if (formatter != null)
                    {
                        formatChar = formatter.GetInFormatChar();
                        if (formatChar == null)
                            throw new InvalidOperationException();
                    }
                    else
                    {
                        formatChar = "?";
                    }
                }

                format.Add(formatChar);
                var a = "&" + structFormatter.GetIndirectedArgName(null, 0) + "->" + field.Name;
                args.Add(a);
            }
        }
    }
}
