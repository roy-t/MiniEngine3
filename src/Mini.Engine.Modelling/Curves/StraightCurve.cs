using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed record class StraightCurve(float Length) : ICurve
{
    public float ComputeLength()
    {
        return this.Length;
    }

    public Vector2 GetForward(float u)
    {
        return Vector2.UnitY;
    }

    public Vector2 GetPosition(float u)
    {
        return Vector2.UnitY * u * this.Length;
    }
}
