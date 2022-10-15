using System.Numerics;

namespace Mini.Engine.Core;

// See: http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/#GeneralizingGoldenRatio

public sealed class QuasiRandomSequence
{
    private readonly int Length;
    private int n;
    

    public QuasiRandomSequence(int length = int.MaxValue, int seed = 0)
    {
        this.Length = length;
        this.n = seed;        
    }

    public float Next1D(float min = 0.0f, float max = 1.0f)
    {
        var g = 1.6180339887498948482f;
        var a1 = 1.0f / g;
        var x = (0.5f + a1 * this.n) % 1.0f;

        this.n = (this.n + 1) % this.Length;
        return TransformRange(x, min, max);
    }

    public Vector2 Next2D(float minX = 0.0f, float maxX = 1.0f, float minY = 0.0f, float maxY = 1.0f)
    {
        var g = 1.32471795724474602596f;
        var a1 = 1.0f / g;
        var a2 = 1.0f / (g * g);

        var x = (0.5f + a1 * this.n) % 1.0f;
        var y = (0.5f + a2 * this.n) % 1.0f;

        x = TransformRange(x, minX, maxX);
        y = TransformRange(y, minY, maxY);

        this.n = (this.n + 1) % this.Length;
        return new Vector2(x, y);
    }

    public Vector3 Next3D(float minX = 0.0f, float maxX = 1.0f, float minY = 0.0f, float maxY = 1.0f, float minZ = 0.0f, float maxZ = 1.0f)
    {
        var g = 1.22074408460575947536f;
        var a1 = 1.0f / g;
        var a2 = 1.0f / (g * g);
        var a3 = 1.0f / (g * g * g);
        var x = (0.5f + a1 * n) % 1.0f;
        var y = (0.5f + a2 * n) % 1.0f;
        var z = (0.5f + a3 * n) % 1.0f;

        x = TransformRange(x, minX, maxX);
        y = TransformRange(y, minY, maxY);
        z = TransformRange(z, minZ, maxZ);

        this.n = (this.n + 1) % this.Length;
        return new Vector3(x, y, z);
    }

    private static float TransformRange(float value, float min, float max)
    {        
        var range = max - min;
        return (value * range) + min;
    }
}
