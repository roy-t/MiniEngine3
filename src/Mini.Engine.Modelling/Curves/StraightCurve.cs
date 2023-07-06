using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class StraightCurve(Vector3 Start, Vector3 Direction, float Length) : ICurve
{
    public float ComputeLength()
    {
        return this.Length;
    }

    public Vector3 GetForward(float u)
    {
        return this.Direction;
    }

    public Vector3 GetPosition(float u)
    {
        return this.Start + (this.Direction * u * this.Length);
    }
}
