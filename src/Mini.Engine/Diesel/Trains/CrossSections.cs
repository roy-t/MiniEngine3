using System.Numerics;
using Mini.Engine.Modelling.Paths;

using static Mini.Engine.Diesel.Trains.TrainParameters;

namespace Mini.Engine.Diesel.Trains;
public static class CrossSections
{

    public static Path2D Circle(float r, int length)
    {
        var vertices = new Vector2[length];
        
        for (var i = 0; i < vertices.Length; i++)
        {
            var step = (MathF.Tau / vertices.Length) * i;
            vertices[i] = new Vector2(MathF.Cos(step) * r, MathF.Sin(step) * r);
        }

        return new Path2D(true, vertices);
    }
}
