using System.Numerics;

namespace Mini.Engine.Core;
public static class Radians
{
    public static float Lerp(float from, float to, float t)
    {
        var retval = from + (Repeat(to - from, 2 * MathF.PI) * t);
        return WrapRadians(retval);
    }

    private static float Repeat(float t, float length)
    {
        return t - (MathF.Floor(t / length) * length);
    }

    /* wrap x -> [-pi, pi) */
    public static float WrapRadians(float radians)
    {
        return WrapRadians(radians, -MathF.PI, MathF.PI);
    }
    private static float FMod(float x, float y)
    {
        return x % y;
    }

    /* wrap x -> [0,max) */
    public static float WrapRadians(float radians, float max)
    {
        return FMod(max + FMod(radians, max), max);
    }

    /* wrap x -> [min,max) */
    public static float WrapRadians(float radians, float min, float max)
    {
        return min + WrapRadians(radians - min, max - min);
    }    

    // converts rotations to a vector where 0 -> (0, -1) aka forward
    public static Vector2 RotationToVector(float radians)
    {
        return new Vector2(MathF.Sin(radians), -MathF.Cos(radians));
    }
}
