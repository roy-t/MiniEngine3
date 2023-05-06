using System.Numerics;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling.Tools;
public static class Extruder
{
    public static Quad[] Extrude(Path2D crossSection, float depth, bool closeShape = true)
    {
        var layout = new Path3D(closeShape, Vector3.Zero, Vector3.UnitZ * -depth);
        return Extrude(crossSection, layout);
    }

    public static Quad[] Extrude(Path2D crossSection, Path3D path)
    {
        if (crossSection.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (path.Length < 2)
        {
            throw new Exception("Invalid path");
        }

        var loops = crossSection.IsClosed ? crossSection.Length + 1 : crossSection.Length;
        var steps = path.IsClosed ? path.Length + 1 : path.Length;

        var positions = new Vector3[steps, loops];

        for (var i = 0; i < steps; i++)
        {
            var position = path[i];
            var face = position + path.GetForwardAlongBendToNextPosition(i);
            var transform = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f);
            transform = transform.FaceTargetConstrained(face, Vector3.UnitY);

            for (var j = 0; j < loops; j++)
            {
                var v2 = crossSection[j % crossSection.Length];
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
