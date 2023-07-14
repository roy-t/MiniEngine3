using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Curves;
public static class CurveExtensions
{
    public const float CurveStart = 0.0f;
    public const float CurveEnd = 1.0f;
    
    public static Vector3 GetLeft(this ICurve curve, float u)
    {
        var forward = curve.GetForward(u);
        return Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));
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

    public static ICurve Reverse(this ICurve curve)
    {
        return new ReverseCurve(curve);
    }

    public static ICurve Translate(this ICurve curve, Vector3 translation)
    {
        return new TranslateCurve(curve, translation);
    }

    public static float ComputeLengthPiecewise(this ICurve curve, int pieces = 1000)
    {
        var distance = 0.0f;
        var step = 1.0f / (pieces - 1.0f);
        for (var i = 0; i < (pieces - 1); i++)
        {

            var a = curve.GetPosition(step * (i + 0));
            var b = curve.GetPosition(step * (i + 1));

            distance += Vector3.Distance(a, b);
        }

        return distance;
    }

    public static ReadOnlySpan<Vector3> GetPoints(this ICurve curve, int points, Vector3 offset)
    {
        var vertices = new ArrayBuilder<Vector3>(points);
        var enumerator = new CurveIterator(curve, points);
        foreach (var position in enumerator)
        {
            vertices.Add(new Vector3(position.X, 0, -position.Y) + offset);
        }

        return vertices.Build();
    }

    public static Matrix4x4 AlignTo(this ICurve curve, float u, Vector3 up)
    {
        var position = curve.GetPosition(u);
        var forward = curve.GetForward(u);
        return new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f)
            .FaceTargetConstrained(position + forward, up)
            .GetMatrix();
    }

    public static (Vector3 Position, Vector3 Normal) GetWorldOrientation(this ICurve curve, float u, Transform transform)
    {
        return GetWorldOrientation(curve, u, transform.GetMatrix());
    }

    public static (Vector3 Position, Vector3 Normal) GetWorldOrientation(this ICurve curve, float u, Matrix4x4 world)
    {
        var position = Vector3.Transform(curve.GetPosition(u), world);
        var forward = Vector3.TransformNormal(curve.GetForward(u), world);

        return (position, forward);
    }
}
