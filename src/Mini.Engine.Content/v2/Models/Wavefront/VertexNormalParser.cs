using System.Numerics;

namespace Mini.Engine.Content.v2.Models.Wavefront;

/// <summary>
/// Specifies a normal vector with components i, j, and k.
/// syntax: vn i j k
/// </summary>
internal sealed class VertexNormalParser : VertexParser
{
    public override string Key => "vn";

    protected override void ParseVertex(ParseState state, Vector4 vertex)
    {
        state.Normals.Add(vertex);
    }
}
