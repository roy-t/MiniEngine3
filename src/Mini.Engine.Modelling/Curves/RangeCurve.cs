using System.Numerics;
using LibGame.Mathematics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class RangeCurve(ICurve Curve, float Start, float Length) : ICurve
{
    public Vector2 GetPosition(float u)
    {
        return this.Curve.GetPosition(this.Rescale(u));
    }    

    public Vector2 GetNormal(float u)
    {
        return this.Curve.GetNormal(this.Rescale(u));
    }

    public float ComputeLength()
    {
        return this.ComputeLengthPiecewise();
    }

    private float Rescale(float u)
    {
        return Ranges.Map(u, (0.0f, 1.0f), (this.Start, this.Start + this.Length));
    }
}
