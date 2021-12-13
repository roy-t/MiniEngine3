using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Parsers;

internal interface IParseState { }

internal interface IStatementParser<T>
    where T : IParseState
{
    bool Parse(T state, ReadOnlySpan<char> line, IVirtualFileSystem fileSystem);
}

internal abstract class StatementParser<T> : IStatementParser<T>
    where T : IParseState
{
    public abstract string Key { get; }

    public bool Parse(T state, ReadOnlySpan<char> line, IVirtualFileSystem fileSystem)
    {
        if (IsRelevant(this.Key, line))
        {
            var arguments = line[(this.Key.Length + 1)..];
            this.ParseArgument(state, arguments, fileSystem);
            this.ParseArguments(state, new SpanTokenEnumerator(arguments), fileSystem);
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

    protected virtual void ParseArguments(T state, SpanTokenEnumerator arguments, IVirtualFileSystem fileSystem) { }
    protected virtual void ParseArgument(T state, ReadOnlySpan<char> argument, IVirtualFileSystem fileSystem) { }
}
