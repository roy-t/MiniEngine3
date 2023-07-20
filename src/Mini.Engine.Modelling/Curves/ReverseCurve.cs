using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class ReverseCurve(ICurve Curve) : ICurve
{
    public float Length => this.Curve.Length;

    public Vector3 GetPosition(float u)
    {
        return this.Curve.GetPosition(1.0f - u);
    }

    public Vector3 GetForward(float u)
    {
        return -this.Curve.GetForward(1.0f - u);
    }
}
