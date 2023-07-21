using System.Numerics;
using Mini.Engine.Modelling.Paths;

using static Mini.Engine.Diesel.Trains.TrainParameters;

namespace Mini.Engine.Diesel.Trains;
public static class CrossSections
{

    public static Path2D Wheel(float r, int length)
    {
        // TODO: check out how much of the bottom part of the wheel we can ignore
        // because it is invisible from the regular camera positions
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
            new Vector2(lmiX, minY),
            new Vector2(minX, midY),
            new Vector2(minX, maxY),
            new Vector2(maxX, maxY),
            new Vector2(maxX, midY),
            new Vector2(rmiX, minY),
        };

        return new Path2D(false, vertices);
    }

    public static Path2D Bed()
    {
        var halfBedLength = FLAT_CAR_LENGTH * 0.5f;
        var halfInnerBedLength = (FLAT_CAR_LENGTH * 0.5f) - FLAT_CAR_BOGEY_GAP_LENGTH;

        var xMin = -halfBedLength;
        var xQ1 = -halfInnerBedLength;
        var xQ2 = halfInnerBedLength;
        var xMax = halfBedLength;

        var yMin = -INNER_WHEEL_RADIUS * 0.6f;
        var yMid = INNER_WHEEL_RADIUS * 0.8f;
        var yMax = INNER_WHEEL_RADIUS * 1.2f;

        // TODO: finish shape of Bed    ;;;;;;;;;;;;;;;;;;;;;;;;;;
        //                               O O ;;;;;;;;;;;;;; O O 

        var vertices = new Vector2[]
        {            
            new Vector2(xMax, yMax),
            new Vector2(xMax, yMid),
            new Vector2(xQ2, yMid),
            new Vector2(xQ2, yMin),
            new Vector2(xQ1, yMin),
            new Vector2(xQ1, yMid),
            new Vector2(xMin, yMid),
            new Vector2(xMin, yMax),
        };

        return new Path2D(true, vertices);
    }
}
