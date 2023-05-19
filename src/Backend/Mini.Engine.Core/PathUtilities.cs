using System.Numerics;
using static System.MathF;
namespace Mini.Engine.Core;
public static class PathUtilities
{
    public static Vector2[] CreateCurve(float radius, float startAngle, float endAngle, int points, bool closed = false)
    {
        var vertices = new Vector2[points];

        var dAngle = endAngle - startAngle;
https://www.desmos.com/calculator/caekbo9ent
        var stepSizeModifier = closed ? 0.0f : 1.0f; // for closed make the step size so that one step is missed in the end
        var step = dAngle / (vertices.Length - stepSizeModifier);
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector2(Cos(step * i), Sin(step * i)) * radius;
        }

        return vertices;
    }

    public static Vector2[] CreateTransitionCurve(float targetRadius, float length, int points)
    {
        // https://youtu.be/dbC-zkSN46k?t=216
        // https://math.stackexchange.com/questions/4702320/how-to-create-a-transition-curve-that-lines-up-with-a-circle-with-a-missing-quar
        var vertices = new Vector2[points];        
        var step = length / (vertices.Length - 1);

        for (var i = 0; i < vertices.Length; i++)
        {
            var s = step * i; // the length of the transition curve from zero to our current position
            var r = targetRadius; // radius of the circular arc
            var L = length; // length of the transition curve

            var x = s - (Pow(s, 5.0f) / (40 * (r * r) * (L * L)));
            var y = (Pow(s, 3) / (6 * r * L)) - (Pow(s, 7) / (336 * Pow(r, 3.0f) * Pow(L, 3)));

            vertices[i] = new Vector2(x, y);
        }

        return vertices;
    }
}
