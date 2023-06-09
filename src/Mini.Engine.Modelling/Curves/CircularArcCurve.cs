using System.Numerics;

using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

public sealed record class CircularArcCurve(float Offset, float Length, float Radius)
    : ICurve
{
    public Vector2 GetPosition(float u)
    {        
        u *= this.Length;
        return new Vector2(Cos(u + this.Offset), Sin(u + this.Offset)) * this.Radius;
    }

    public float ComputeLength()
    {                
        return this.Length * this.Radius;
    }

    public Vector2 GetForward(float u)
    {
        u *= this.Length;

        // Normalize to get rid of floating point inaccuracies
        return Vector2.Normalize(new Vector2(-Sin(u + this.Offset), Cos(u + this.Offset)));
    }
}
