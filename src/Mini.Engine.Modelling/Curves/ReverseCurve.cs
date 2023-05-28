using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class ReverseCurve(ICurve Curve) : ICurve
{
    public Vector2 GetPosition(float u)
    {
        return this.Curve.GetPosition(1.0f - u);
    }

    public Vector2 GetNormal(float u)
    {
        return -this.Curve.GetNormal(1.0f - u);
    }

    public float ComputeLength()
    {
        return this.Curve.ComputeLength();
    }
}
