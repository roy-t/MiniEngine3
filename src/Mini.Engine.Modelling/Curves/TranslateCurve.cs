using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class TranslateCurve(ICurve Curve, Vector2 Translation) : ICurve
{
    public Vector2 GetPosition(float u)
    {
        return this.Curve.GetPosition(u) + this.Translation;
    }

    public Vector2 GetForward(float u)
    {
        return this.Curve.GetForward(u);
    }

    public float ComputeLength()
    {
        return this.Curve.ComputeLength();
    }
}
