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
        public override string GetFormatChar() => "O";
        public override string DeclareParseArgTupleInputConverter() => $"PyObject *ob{Parameter.Name};";
        public override string GetParseTupleArg() => $"&ob{Parameter.Name}";
        //public override IEnumerable<string> GetParsePostCode() { yield return $"if (bPythonIsHappy && !PyWinObject_As{Parameter.Type.Name}(ob{Parameter.Name}, &{GetIndirectedArgName(null, 0)}, FALSE) bPythonIsHappy = FALSE;"; }
        public override IEnumerable<string> GetParsePostCode()
        {
            if (Structure.Fields.Count == 1)
                yield return $"if (bPythonIsHappy && !PyWinObject_As{Parameter.Type.Name}(ob{Parameter.Name}, &{GetIndirectedArgName(null, 0)}, FALSE) bPythonIsHappy = FALSE;";

            var format = new List<string>();
            var args = new List<string>();

            BuildParsePostCode(Structure, format, args);
            yield return $"if (bPythonIsHappy && !PyArg_ParseTuple(ob{Parameter.Name}, \"{string.Join(string.Empty, format)}\", { string.Join(", ", args)}) bPythonIsHappy = FALSE;";
        }

        private void BuildParsePostCode(ParsedStructure structure, List<string> format, List<string> args)
        {
            foreach (var field in Structure.Fields)
            {
                var pp = new ParsedParameter(null, field.Name, ParameterAttributes.None, 0);
                pp.Type = field.Type.Clone();
                var formatter = GetArgConverter(Context, pp);
                string? formatChar;
                if (formatter != null)
                {
                    formatChar = formatter.GetFormatChar();
                    if (formatChar == null)
                        throw new InvalidOperationException();
                }
                else
                {
                    formatChar = "?";
                }

                format.Add(formatChar);
                var a = "&" + GetIndirectedArgName(null, 0) + "->" + field.Name;
                args.Add(a);
            }
        }
    }
}
