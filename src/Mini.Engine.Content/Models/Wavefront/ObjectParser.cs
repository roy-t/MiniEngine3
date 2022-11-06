using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront.Objects;

/// <summary>
/// Object name statements let you assign a name to an entire object in a single file.
/// syntax: o object_name
/// </summary>
internal sealed class ObjectParser : ObjStatementParser
{
    public override string Key => "o";
    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument, IReadOnlyVirtualFileSystem fileSystem)
    {
        state.Object = argument.ToString();
    }
}
