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
            var step = (MathF.Tau / vertices.Length) * -i;
            vertices[i] = new Vector2(MathF.Cos(step) * r, MathF.Sin(step) * r);
        }

        return new Path2D(true, vertices);
    }

    public static Path2D Plate()
    {        
        var maxY = INNER_WHEEL_RADIUS * 0.6f;
        var midY = 0.0f;
        var minY = -INNER_WHEEL_RADIUS * 0.6f;

        var minX = (WHEEL_SPACING * -0.5f) - INNER_WHEEL_RADIUS * 0.8f;
        var lmiX = (WHEEL_SPACING * -0.5f);
        var rmiX = (WHEEL_SPACING * +0.5f);
        var maxX = (WHEEL_SPACING * +0.5f) + INNER_WHEEL_RADIUS * 0.8f;

        var vertices = new Vector2[]
        {
            new Vector2(maxX, maxY),
            new Vector2(maxX, midY),
            new Vector2(rmiX, minY),
            new Vector2(lmiX, minY),
            new Vector2(minX, midY),
            new Vector2(minX, maxY),
        };

        return new Path2D(true, vertices);
    }
}
