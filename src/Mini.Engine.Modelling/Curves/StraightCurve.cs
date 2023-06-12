using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class StraightCurve(Vector2 Start, Vector2 Direction, float Length) : ICurve
{
    public float ComputeLength()
    {
        return this.Length;
    }

    public Vector2 GetForward(float u)
    {
        return this.Direction;
    }

    public Vector2 GetPosition(float u)
    {
        return this.Start + (this.Direction * u * this.Length);
    }
}
