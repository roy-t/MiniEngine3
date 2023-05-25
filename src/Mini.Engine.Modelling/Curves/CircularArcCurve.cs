using System.Numerics;

using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

/// <summary>
/// A circular arc curve
/// </summary>
/// <param name="StartAngle"></param>
/// <param name="EndAngle"></param>
/// <param name="Closed"></param>
public sealed record class CircularArcCurve(float StartAngle, float EndAngle, bool Closed = false)
    : ICurve
{
    public Vector2 GetPosition(float u, float radius)
    {
        var delta = this.EndAngle - this.StartAngle;
        u *= delta;
        return new Vector2(Cos(u + this.StartAngle), Sin(u + this.StartAngle)) * radius;
    }

    public float ComputeLength(float radius)
    {
        var delta = this.EndAngle - this.StartAngle;
        return (2.0f * PI * radius) * delta;
    }
}
