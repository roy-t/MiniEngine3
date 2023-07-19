using System.Numerics;

using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

public sealed record class CircularArcCurve(float Offset, float Radians, float Radius)
    : ICurve
{
    public Vector3 GetPosition(float u)
    {        
        u *= this.Radians;
        return new Vector3(Cos(u + this.Offset), 0.0f, Sin(u + this.Offset)) * this.Radius;
    }

    public float ComputeLength()
    {        
        return this.Radians * this.Radius;
    }

    public Vector3 GetForward(float u)
    {
        u *= this.Radians;

        // Normalize to get rid of floating point inaccuracies
        return Vector3.Normalize(new Vector3(-Sin(u + this.Offset), 0.0f, Cos(u + this.Offset)));
    }
}
