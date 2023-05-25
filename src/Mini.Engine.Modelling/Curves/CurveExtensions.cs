using System.Numerics;

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

    public static Vector3 GetLeft(this ICurve curve, float u)
    {
        var n = curve.GetNormal3D(u);
        return Vector3.Normalize(Vector3.Cross(Vector3.UnitY, n));
    }
}
