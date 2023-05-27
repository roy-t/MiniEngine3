using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Curves;
public static class CurveExtensions
{
    public static Vector3 GetPosition3D(this ICurve curve, float u)
    {
        var p = curve.GetPosition(u);
        return new Vector3(p.X, 0.0f, -p.Y);
    }

    public static Vector3 GetNormal3D(this ICurve curve, float u)
    {
        var p = curve.GetNormal(u);
        return new Vector3(p.X, 0.0f, -p.Y);
    }

    public static Vector2 GetLeft(this ICurve curve, float u)
    {
        var n = curve.GetNormal(u);
        return new Vector2(-n.Y, n.X);
    }

    public static Vector3 GetLeft3D(this ICurve curve, float u)
    {
        var n = curve.GetNormal3D(u);
        return Vector3.Normalize(Vector3.Cross(Vector3.UnitY, n));
    }

    public static ICurve OffsetLeft(this ICurve curve, float offset)
    {
        return new OffsetCurve(curve, offset);
    }

    public static ICurve OffsetRight(this ICurve curve, float offset)
    {
        return new OffsetCurve(curve, -offset);
    }

    public static ICurve Range(this ICurve curve, float start, float length)
    {
        return new RangeCurve(curve, start, length);
    }

    public static float ComputeLengthPiecewise(this ICurve curve, int pieces = 1000)
    {
        var distance = 0.0f;
        var step = 1.0f / (pieces - 1.0f);
        for (var i = 0; i < (pieces - 1); i++)
        {

            var a = curve.GetPosition(step * (i + 0));
            var b = curve.GetPosition(step * (i + 1));

            distance += Vector2.Distance(a, b);
        }

        return distance;
    }

    public static IReadOnlyList<Vector2> GetPoints(this ICurve curve, int points, Vector2 offset)
    {
        var vertices = new List<Vector2>(points);
        var enumerator = new CurveIterator(curve, points);
        foreach (var position in enumerator)
        {
            vertices.Add(position + offset);
        }

        return vertices;
    }

    public static ReadOnlySpan<Vector3> GetPoints3D(this ICurve curve, int points, Vector3 offset)
    {
        var vertices = new ArrayBuilder<Vector3>(points);
        var enumerator = new CurveIterator(curve, points);
        foreach (var position in enumerator)
        {
            vertices.Add(new Vector3(position.X, 0, -position.Y) + offset);
        }

        return vertices.Build();
    }
}
