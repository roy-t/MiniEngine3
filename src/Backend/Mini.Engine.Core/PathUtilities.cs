using System.Numerics;

namespace Mini.Engine.Core;
public static class PathUtilities
{
    public static Vector2[] CreateCurve(float radius, float startAngle, float endAngle, int points, bool closed = false)
    {
        var vertices = new Vector2[points];

        var dAngle = endAngle - startAngle;

        var stepSizeModifier = closed ? 0.0f : 1.0f; // for closed make the step size so that one step is missed in the end
        var step = dAngle / (vertices.Length - stepSizeModifier);
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector2(MathF.Cos(step * i), MathF.Sin(step * i)) * radius;
        }

        return vertices;
    }


    public static Vector2[] CreateTransitionCurve(float radius, float end)
    {
        // https://youtu.be/dbC-zkSN46k?t=216
    }
}
