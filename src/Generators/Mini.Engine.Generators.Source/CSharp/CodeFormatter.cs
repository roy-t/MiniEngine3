using System.Text;

namespace Mini.Engine.Generators.Source.CSharp;

public sealed class FormatOptions
{
    public static FormatOptions Default => new();

    public int IndentationWidth { get; set; } = 4;
}

public static class CodeFormatter
{
    public static string Format(string code, FormatOptions options)
    {
        return IndentEachScope(RemoveDuplicateNewLines(code), options);
    }

    private static string RemoveDuplicateNewLines(string code)
    {
        var builder = new StringBuilder();
        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        var wasEmpty = false;

        foreach(var line in lines)
        {
            var isEmpty = string.IsNullOrWhiteSpace(line);

            if (!isEmpty || !wasEmpty)
            {
                builder.AppendLine(line.Trim());
            }

            wasEmpty = isEmpty;
        }

        return builder.ToString();
    }

    private static string IndentEachScope(string code, FormatOptions options)
    {
        var builder = new StringBuilder();

        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        var indentation = 0;
        foreach (var line in lines)
        {
            var openScope = line.Contains('{') && !line.Contains('}');
            var closeScope = line.Contains('}') && !line.Contains('{');

            if (closeScope) { indentation--; }

            builder.Append(' ', indentation * options.IndentationWidth);
            builder.AppendLine(line.Trim());

            if (openScope) { indentation++; }
        }

        return builder.ToString();
    } 
}