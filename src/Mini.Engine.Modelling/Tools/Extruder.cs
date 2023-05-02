using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Tools;
public static class Extruder
{
    public static Quad[] Extrude(Shape crossSection, float depth)
    {
        if (crossSection.Vertices.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        var quads = new Quad[crossSection.Vertices.Length];

        for (var i = 0; i < crossSection.Vertices.Length; i++)
        {
            var a = crossSection.Vertices[i];
            var b = crossSection.Vertices[(i + 1) % crossSection.Vertices.Length];
            var n = LineMath.GetNormalFromLineSegement(a, b);
            var normal = new Vector3(n.X, n.Y, 0.0f);
            quads[i] = new Quad(normal, new Vector3(b.X, b.Y, -depth), new Vector3(b.X, b.Y, 0.0f), new Vector3(a.X, a.Y, 0.0f), new Vector3(a.X, a.Y, -depth));
        }

        return quads;
    }
}
