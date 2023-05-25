using System.Diagnostics;
using System.Numerics;
using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

/// <summary>
/// A polynomial curve: (x,y) = (a + bu + cu^2 + du^3, e + fu + gu^2 + hu^3)
/// </summary>
public sealed record class PolynomialCurve(float A, float B, float C, float D, float E, float F, float G, float H)
    : ICurve
{
    public Vector2 GetPosition(float u, float amplitude)
    {
        Debug.Assert(u >= 0.0f && u <= 1.0f);
        Debug.Assert(amplitude > 0.0f);

        var u2 = Pow(u, 2.0f);
        var u3 = Pow(u, 3.0f);

        var x = this.A + (this.B * u) + (this.C * u2) + (this.D * u3);
        var y = this.E + (this.F * u) + (this.G * u2) + (this.H * u3);

        return new Vector2(x, y) * amplitude;
    }

    public static PolynomialCurve CreateTransitionCurve()
    {
        return new PolynomialCurve(-1.0f, 3.0f / 2.0f, 3.0f, -(5.0f / 2.0f), 1.0f, 0.0f, 0.0f, -1.0f);
    }

    public float ComputeLength(float amplitude)
    {
        throw new NotImplementedException(); // TODO
    }
}