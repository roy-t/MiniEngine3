using System.Numerics;
using static System.MathF;
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
            vertices[i] = new Vector2(Cos((step * i) + startAngle), Sin((step * i) + startAngle)) * radius;
        }

        return vertices;
    }

    public static Vector2[] CreateTransitionCurve(float radius, int points)
    {
        // https://youtu.be/dbC-zkSN46k?t=216
        // https://math.stackexchange.com/questions/4702320/how-to-create-a-transition-curve-that-lines-up-with-a-circle-with-a-missing-quar
        var R = radius;

        var a = -R;
        var b = (3.0f / 2.0f) * R;
        var c = 3.0f * R;
        var d = -(5.0f / 2.0f) * R;
        var e = R;
        var f = 0.0f;
        var g = 0.0f;
        var h = -R;

        var vertices = new Vector2[points];
        var step = 1.0f / (points - 1.0f);
        for (var i = 0; i < points; i++)
        {
            var u = i * step;
            var u2 = Pow(u, 2);
            var u3 = Pow(u, 3);
            var x = a + (b * u) + (c * u2) + (d * u3);
            var y = e + (f * u) + (g * u2) + (h * u3);

            vertices[i] = new Vector2(x, -y);
        }

        return vertices;
    }
}
