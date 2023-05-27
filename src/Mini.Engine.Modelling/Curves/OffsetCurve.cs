using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class OffsetCurve(ICurve Curve, float Offset) : ICurve
{
    public Vector2 GetPosition(float u)
    {
        var p = this.Curve.GetPosition(u);
        var l = this.Curve.GetLeft(u);

        return p + (l * this.Offset);
    }

    public Vector2 GetNormal(float u)
    {
        return this.Curve.GetNormal(u);
    }

    public float ComputeLength()
    {
        return this.ComputeLengthPiecewise(1000);
    }
}
