using System;

namespace Mini.Engine.Content.Models.Obj;

/// <summary>
/// Specifies the material name for the element following it. Once a material is assigned, it cannot be turned off; it can only be changed.
/// syntax: usemtl material_name
/// </summary>
internal sealed class UseMtlParser : StatementParser
{
    public override string Key => "usemtl";
    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument)
    {
        state.Material = argument.ToString();
    }
}
