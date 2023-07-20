using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class OffsetCurve(ICurve Curve, float Offset) : ICurve
{
    public float Length => this.Curve.Length;

    public Vector3 GetPosition(float u)
    {
        var p = this.Curve.GetPosition(u);
        var l = this.Curve.GetLeft(u);

        return p + (l * this.Offset);
    }

    public Vector3 GetForward(float u)
    {
        return this.Curve.GetForward(u);
    }    
}
