using System.Numerics;
using LibGame.Geometry;
using LibGame.Mathematics;
using Mini.Engine.Graphics.Primitives;
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
        for (var i = 0; i < points; i++)
        {
            var u = i / (points - 1.0f);
            var matrix = curve.AlignTo(u, up);

            for (var j = 0; j < crossSection.Steps; j++)
            {
                var a = crossSection[j + 0];
                var b = crossSection[j + 1];

                var normal = Vector3.TransformNormal(Lines.GetNormalFromLineSegement(a, b).Expand(), matrix);

                var vA = Vector3.Transform(a.Expand(), matrix);
                var vB = Vector3.Transform(b.Expand(), matrix);

                var index = builder.AddVertex(vA, normal);
                minIndex = Math.Min(minIndex, index);

                index = builder.AddVertex(vB, normal);
                minIndex = Math.Min(minIndex, index);
            }
        }

        var verticesPerStep = 2;
        var verticesPerLoop = crossSection.Steps * verticesPerStep;
        for (var extrudeI = 0; extrudeI < points - 1; extrudeI++)
        {
            var cLoop = minIndex + ((extrudeI + 0) * verticesPerLoop);
            var nLoop = minIndex + ((extrudeI + 1) * verticesPerLoop);

            for (var loopI = 0; loopI < crossSection.Steps; loopI++)
            {
                var offset = loopI * verticesPerStep;
                var tl = nLoop + offset + 0;
                var tr = nLoop + ((offset + 1) % verticesPerLoop);

                var bl = cLoop + offset + 0;
                var br = cLoop + ((offset + 1) % verticesPerLoop);

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
                var topRight = Vector3.Transform(crossSection[j + 1].Expand(), matrixB);
                var bottomRight = Vector3.Transform(crossSection[j + 1].Expand(), matrixA);
                var bottomLeft = Vector3.Transform(crossSection[j].Expand(), matrixA);
                var topLeft = Vector3.Transform(crossSection[j].Expand(), matrixB);

                builder.AddQuad(topRight, bottomRight, bottomLeft, topLeft);
            }
        }
    }
}
