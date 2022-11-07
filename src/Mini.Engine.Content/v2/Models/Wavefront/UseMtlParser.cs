using Mini.Engine.IO;

namespace Mini.Engine.Content.v2.Models.Wavefront;

/// <summary>
/// Specifies the material name for the element following it. Once a material is assigned, it cannot be turned off; it can only be changed.
/// syntax: usemtl material_name
/// </summary>
internal sealed class UseMtlParser : ObjStatementParser
{
    public override string Key => "usemtl";
    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument, IReadOnlyVirtualFileSystem fileSystem)
    {
        var material = new string(argument);
        if (string.IsNullOrEmpty(state.Material))
        {
            state.Material = material;
        }
        else
        {
            // obj permits one group to use multiple materials, while we use 1 material per group
            // so start a new group whenever a new material is encountered
            state.NewGroup($"{state.Group?.Name ?? "unknown"}_{material}");
            state.Material = material;
        }
    }
}
