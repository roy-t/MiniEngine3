using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront.Materials;

internal class NewMtlParser : MtlStatementParser
{
    public override string Key => "newmtl";

    protected override void ParseArgument(MaterialParseState state, ReadOnlySpan<char> argument, IVirtualFileSystem fileSystem)
    {
        state.NewMaterial(new string(argument));
    }
}
