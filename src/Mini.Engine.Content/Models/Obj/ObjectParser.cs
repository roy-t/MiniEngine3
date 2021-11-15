using System;

namespace Mini.Engine.Content.Models.Obj;

/// <summary>
/// Object name statements let you assign a name to an entire object in a single file.
/// syntax: o object_name
/// </summary>
internal sealed class ObjectParser : StatementParser
{
    public override string Key => "o";
    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument)
    {
        state.Object = argument.ToString();
    }
}
