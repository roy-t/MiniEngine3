using System.Numerics;

namespace Mini.Engine.Content.Models.Wavefront.Objects;

/// <summary>
/// Specifies a geometric vertex and its x y z coordinates. Rational curves and surfaces require a fourth homogeneous coordinate, also called the weight.
/// syntax: v x y z w
/// </summary>
internal sealed class VertexPositionParser : VertexParser
{
    public override string Key => "v";

    protected override void ParseVertex(ObjectParseState state, Vector4 vertex)
    {
        state.Vertices.Add(vertex);
    }
}
