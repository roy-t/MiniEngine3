using System.Numerics;

using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

/// <summary>
/// A circular arc curve
/// </summary>
/// <param name="StartAngle"></param>
/// <param name="EndAngle"></param>
/// <param name="Closed"></param>
public sealed record class CircularArcCurve(float StartAngle, float EndAngle, float Radius, bool Closed = false)
    : ICurve
{
    public Vector2 GetPosition(float u)
    {
        var delta = this.EndAngle - this.StartAngle;
        u *= delta;
        return new Vector2(Cos(u + this.StartAngle), Sin(u + this.StartAngle)) * this.Radius;
    }

    public float ComputeLength()
    {        
        var delta = this.EndAngle - this.StartAngle;
        return delta * this.Radius;
    }

    public Vector2 GetNormal(float u)
    {
        var delta = this.EndAngle - this.StartAngle;
        u *= delta;

        // Normalize to get rid of floating point inaccuracies
        return Vector2.Normalize(new Vector2(-Sin(u + this.StartAngle), Cos(u + this.StartAngle)));
    }
}
