using System.Numerics;

namespace Mini.Engine.Content.Models.Wavefront.Objects;

/// <summary>
/// Specifies a texture vertex and its coordinates. A 1D texture requires only u texture coordinates, a 2D texture requires both u and v texture coordinates, and a 3D texture requires all three coordinates.
/// syntax: vt u v w
/// </summary>
internal sealed class VertexTextureParser : VertexParser
{
    public override string Key => "vt";

    protected override void ParseVertex(ObjectParseState state, Vector4 vertex)
    {
        state.Texcoords.Add(vertex);
    }
}
