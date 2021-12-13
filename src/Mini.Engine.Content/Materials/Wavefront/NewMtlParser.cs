using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials.Wavefront;

internal class NewMtlParser : MtlStatementParser
{
    public override string Key => "newmtl";

    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument, IVirtualFileSystem fileSystem)
    {
        state.NewMaterial(new string(argument));
    }
}
