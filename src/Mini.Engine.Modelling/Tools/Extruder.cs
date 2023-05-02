using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling.Tools;
public static class Extruder
{
    public static Quad[] Extrude(Shape crossSection, float depth, bool closeShape = true)
    {
        var layout = new Vector3[] { Vector3.Zero, Vector3.UnitZ * -depth };
        return Extrude(crossSection, layout, closeShape);
    }

    // TODO: should layout be (position, normal)[] so that we can usee FaceTargetConstrained?
    // What about twisting?
    public static Quad[] Extrude(Shape crossSection, ReadOnlySpan<Vector3> layout, bool closeCrossSection = true)
    {
        if (crossSection.Vertices.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (layout.Length < 2)
        {
            throw new Exception("Invalid layout");
        }

        var loops = closeCrossSection ? crossSection.Vertices.Length + 1 : crossSection.Vertices.Length;
        var steps = layout.Length;

        var positions = new Vector3[steps, loops];

        for (var i = 0; i < steps; i++)
        {
            var transform = Transform.Identity;
            if (i > 0)
            {
                var start = layout[i];
                Vector3 finish;
                if ((i + 1) < layout.Length)
                {
                    finish = layout[i + 1];
                }
                else
                {
                    finish = layout[i] + Vector3.Normalize(layout[i] - layout[i - 1]);
                }
                transform = transform.AddTranslation(start);
                transform = transform.FaceTarget(finish);
            }

            for (var j = 0; j < loops; j++)
            {
                var v2 = crossSection.Vertices[j % crossSection.Vertices.Length];
                var v3 = new Vector3(v2.X, v2.Y, 0.0f);
                positions[i, j] = Vector3.Transform(v3, transform.GetMatrix());
            }
        }

        var quads = new Quad[(positions.GetLength(0) - 1) * (positions.GetLength(1) - 1)];

        var q = 0;
        for (var i = 0; i < positions.GetLength(0) - 1; i++)
        {            
            for (var j = 0; j < positions.GetLength(1) - 1; j++)
            {
                var a = positions[i + 1, j + 1];
                var b = positions[i + 0, j + 1];
                var c = positions[i + 0, j + 0];
                var d = positions[i + 1, j + 0];

                var plane = Plane.CreateFromVertices(a, b, c);
                var normal = plane.Normal;

                var quad = new Quad(normal, a, b, c, d);
                quads[q++] = quad;
            }
        }

        return quads;
    }
}
