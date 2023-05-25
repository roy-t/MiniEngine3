using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

/// <summary>
/// A polynomial curve: (x,y) = (a + bu + cu^2 + du^3, e + fu + gu^2 + hu^3)
/// </summary>
public sealed record class PolynomialCurve(float A, float B, float C, float D, float E, float F, float G, float H, float Amplitude)
    : ICurve
{
    public Vector2 GetPosition(float u)
    {
        Debug.Assert(u >= 0.0f && u <= 1.0f);
        Debug.Assert(this.Amplitude > 0.0f);

        var u2 = Pow(u, 2.0f);
        var u3 = Pow(u, 3.0f);

        var x = this.A + (this.B * u) + (this.C * u2) + (this.D * u3);
        var y = this.E + (this.F * u) + (this.G * u2) + (this.H * u3);

        return new Vector2(x, y) * this.Amplitude;
    }

    public Vector2 GetNormal(float u)
    {
        // The differentiation of the polynomial curve gives
        // b + 2cu + 3du^2,  f + 2gu + 3hu^2

        Debug.Assert(u >= 0.0f && u <= 1.0f);
        Debug.Assert(this.Amplitude > 0.0f);

        var u2 = Pow(u, 2.0f);

        var x = this.B + (2.0f * this.C * u) + (3.0f * this.D * u2);
        var y = this.F + (2.0f * this.G * u) + (3.0f * this.H * u2);

        // Normalize to get rid of floating point inaccuracies
        return Vector2.Normalize(new Vector2(x, y));
    }

    public static PolynomialCurve CreateTransitionCurve(float amplitude)
    {
        return new PolynomialCurve(-1.0f, 3.0f / 2.0f, 3.0f, -(5.0f / 2.0f), 1.0f, 0.0f, 0.0f, -1.0f, amplitude);
    }

    // TODO: very naive, there must be a better way
    public float ComputeLength()
    {
        const int steps = 1000;
        
        var distance = 0.0f;
        var step = 1.0f / (steps - 1.0f);
        for (var i = 0; i < (steps - 1); i++)
        {

            var a = this.GetPosition(step * (i + 0));
            var b = this.GetPosition(step * (i + 1));

            distance += Vector2.Distance(a, b);
        }

        return distance;
    }


}