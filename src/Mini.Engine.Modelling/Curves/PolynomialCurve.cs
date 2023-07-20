using System.Diagnostics;
using System.Numerics;
using static System.MathF;

namespace Mini.Engine.Modelling.Curves;

/// <summary>
/// A polynomial curve: (x, 0, z) = (a + bu + cu^2 + du^3, e + fu + gu^2 + hu^3)
/// </summary>
public sealed class PolynomialCurve
    : ICurve
{
    public PolynomialCurve(float a, float b, float c, float d, float e, float f, float g, float h, float amplitude)
    {        
        this.A = a;
        this.B = b;
        this.C = c;
        this.D = d;
        this.E = e;
        this.F = f;
        this.G = g;
        this.H = h;
        this.Amplitude = amplitude;

        // TODO: very naive, there is a better way for polynomials like this
        this.Length = this.ComputeLengthPiecewise();
    }

    public float Length { get; }
    public float A { get; }
    public float B { get; }
    public float C { get; }
    public float D { get; }
    public float E { get; }
    public float F { get; }
    public float G { get; }
    public float H { get; }
    public float Amplitude { get; }   

    public Vector3 GetPosition(float u)
    {
        Debug.Assert(u >= 0.0f && u <= 1.0f);
        Debug.Assert(this.Amplitude > 0.0f);

        var u2 = Pow(u, 2.0f);
        var u3 = Pow(u, 3.0f);

        var x = this.A + (this.B * u) + (this.C * u2) + (this.D * u3);
        var z = this.E + (this.F * u) + (this.G * u2) + (this.H * u3);

        return new Vector3(x,.0f, z) * this.Amplitude;
    }

    public Vector3 GetForward(float u)
    {
        // The differentiation of the polynomial curve gives
        // b + 2cu + 3du^2,  f + 2gu + 3hu^2

        Debug.Assert(u >= 0.0f && u <= 1.0f);
        Debug.Assert(this.Amplitude > 0.0f);

        var u2 = Pow(u, 2.0f);

        var x = this.B + (2.0f * this.C * u) + (3.0f * this.D * u2);
        var z = this.F + (2.0f * this.G * u) + (3.0f * this.H * u2);

        // Normalize to get rid of floating point inaccuracies
        return Vector3.Normalize(new Vector3(x, 0.0f, z));
    }

    public static PolynomialCurve CreateTransitionCurve(float amplitude)
    {
        return new PolynomialCurve(-1.0f, 3.0f / 2.0f, 3.0f, -(5.0f / 2.0f), 1.0f, 0.0f, 0.0f, -1.0f, amplitude);
    }
}