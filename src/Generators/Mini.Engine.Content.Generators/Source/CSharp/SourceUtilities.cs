using Microsoft.CodeAnalysis.CSharp;

namespace Mini.Engine.Content.Generators.Source.CSharp
{
    public static class SourceUtilities
    {
        public static string CapitalizeFirstLetter(string text)
            => char.ToUpper(text[0]) + text.Substring(1);

        public static string LowerCaseFirstLetter(string text)
            => char.ToLower(text[0]) + text.Substring(1);


        public static string ToLiteral(string text)
            => SymbolDisplay.FormatLiteral(text, true);

        public static string ToMultilineLiteral(string text)
        {
            var doubleQuoted = text.Replace("\"", "\"\"");
            return $"@\"{doubleQuoted}\"";
        }
    }
}
