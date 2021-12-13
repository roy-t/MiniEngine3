using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront.Objects;

/// <summary>
/// Specifies the material name for the element following it. Once a material is assigned, it cannot be turned off; it can only be changed.
/// syntax: usemtl material_name
/// </summary>
internal sealed class UseMtlParser : ObjStatementParser
{
    public override string Key => "usemtl";
    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument, IVirtualFileSystem fileSystem)
    {
        state.Material = state.Materials[new string(argument)];
    }
}
