using System.Numerics;
using LibGame.Mathematics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;

namespace Mini.Engine.Modelling.Tools;
public static class Extruder
{   
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
