using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Core;
public static class Ranges
{
    public static float Map(float value, (float min, float max) sourceRange, (float min, float max) targetRange)
    {
        value = Math.Clamp(value, sourceRange.min, sourceRange.max);

        var deltaSource = sourceRange.max - sourceRange.min;
        Debug.Assert(deltaSource > 0);

        var deltaTarget = targetRange.max - targetRange.min;
        Debug.Assert(deltaTarget > 0);

        return ((value - sourceRange.min) / deltaSource * deltaTarget) + targetRange.min;
    }

    public static Vector2 Map(Vector2 value, (float min, float max) sourceRange, (float min, float max) targetRange)
    {
        return Map(value, sourceRange, targetRange, sourceRange, targetRange);
    }

    public static Vector2 Map(Vector2 value, (float min, float max) xSourceRange, (float min, float max) xTargetRange, (float min, float max) ySourceRange, (float min, float max) yTargetRange)
    {
        var x = Map(value.X, xSourceRange, xTargetRange);
        var y = Map(value.Y, ySourceRange, yTargetRange);

        return new Vector2(x, y);
    }
}
