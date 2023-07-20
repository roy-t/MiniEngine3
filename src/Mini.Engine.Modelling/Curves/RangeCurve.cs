using System.Numerics;
using LibGame.Mathematics;

namespace Mini.Engine.Modelling.Curves;
public sealed class RangeCurve : ICurve
{
    public readonly ICurve Curve;
    public readonly float UStart;
    public readonly float ULength;

    public RangeCurve(ICurve curve, float uStart, float uLength)
    {
        this.Curve = curve;
        this.UStart = uStart;
        this.ULength = uLength;
        
        this.Length = this.ComputeLengthPiecewise(); 
    }

    public float Length { get; }

    public Vector3 GetPosition(float u)
    {
        return this.Curve.GetPosition(this.Rescale(u));
    }    

    public Vector3 GetForward(float u)
    {
        return this.Curve.GetForward(this.Rescale(u));
    }

    private float Rescale(float u)
    {
        return Ranges.Map(u, (0.0f, 1.0f), (this.UStart, this.UStart + this.ULength));
    }
}
