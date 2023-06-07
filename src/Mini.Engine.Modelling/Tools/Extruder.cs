using System.Numerics;
using LibGame.Geometry;
using LibGame.Mathematics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;

namespace Mini.Engine.Modelling.Tools;
public static class Extruder
{
    public static void ExtrudeSmooth(IPrimitiveMeshPartBuilder builder, Path2D crossSection, ICurve curve, int points, Vector3 up)
    {
        if (crossSection.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (points < 2)
        {
            throw new Exception("Invalid points");
        }

        var minIndex = int.MaxValue;
        var steps = points - 1;
        for (var i = 0; i < points; i++)
        {
            var u = i / (float)steps;
            var matrix = curve.AlignTo(u, up);

            for (var j = 0; j < crossSection.Length; j++)
            {
                Vector3 normal;
                if ((j + 1) < crossSection.Length)
                {
                    normal = Vector3.TransformNormal(Lines.GetNormalFromLineSegement(crossSection[j], crossSection[j + 1]).WithZ(), matrix);
                }
                else
                {
                    normal = Vector3.TransformNormal(Lines.GetNormalFromLineSegement(crossSection[j - 1], crossSection[j]).WithZ(), matrix);
                }

                var vertex = Vector3.Transform(crossSection[j].WithZ(), matrix);
                var index = builder.AddVertex(vertex, normal);
                minIndex = Math.Min(minIndex, index);
            }
        }

        for (var i = 0; i < steps; i++)
        {
            var currentLoop = minIndex + (i * crossSection.Length);
            var nextLoop = minIndex + ((i + 1) * crossSection.Length);

            for (var j = 0; j < crossSection.Steps; j++)
            {
                var tl = nextLoop + j + 0;
                var tr = nextLoop + ((j + 1) % crossSection.Steps);

                var bl = currentLoop + j + 0;
                var br = currentLoop + ((j + 1) % crossSection.Steps); ;

                builder.AddIndex(tr);
                builder.AddIndex(br);
                builder.AddIndex(bl);

                builder.AddIndex(bl);
                builder.AddIndex(tl);
                builder.AddIndex(tr);
            }
        }
    }

    public static void Extrude(IPrimitiveMeshPartBuilder builder, Path2D crossSection, ICurve curve, int points, Vector3 up)
    {
        if (crossSection.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (points < 2)
        {
            throw new Exception("Invalid points");
        }

        var steps = points - 1;
        for (var i = 0; i < steps; i++)
        {
            var uC = (i + 0) / (float)steps;
            var matrixA = curve.AlignTo(uC, up);

            var uN = (i + 1) / (float)steps;
            var matrixB = curve.AlignTo(uN, up);

            for (var j = 0; j < crossSection.Steps; j++)
            {
                var topRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixB);
                var bottomRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixA);
                var bottomLeft = Vector3.Transform(crossSection[j].WithZ(), matrixA);
                var topLeft = Vector3.Transform(crossSection[j].WithZ(), matrixB);

                builder.AddQuad(topRight, bottomRight, bottomLeft, topLeft);
            }
        }
    }
}
