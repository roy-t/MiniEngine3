using System;

namespace Mini.Engine.Content.Models.Obj;

internal interface IStatementParser
{
    bool Parse(ParseState state, ReadOnlySpan<char> line);
}

internal abstract class StatementParser : IStatementParser
{
    public abstract string Key { get; }

    public bool Parse(ParseState state, ReadOnlySpan<char> line)
    {
        if (IsRelevant(this.Key, line))
        {
            var arguments = line[this.Key.Length..];
            this.ParseArgument(state, arguments);
            this.ParseArguments(state, new SpanTokenEnumerator(arguments));
            return true;
        }

        return false;
    }

    private static bool IsRelevant(string key, ReadOnlySpan<char> line)
    {
        return line.StartsWith(key.AsSpan(), StringComparison.InvariantCultureIgnoreCase) &&
            line.Length > key.Length &&
            char.IsWhiteSpace(line[key.Length]);
    }

    protected virtual void ParseArguments(ParseState state, SpanTokenEnumerator arguments) { }
    protected virtual void ParseArgument(ParseState state, ReadOnlySpan<char> argument) { }
}
