using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Core;
public static class LineMath
{
    public static Vector2 GetNormalFromLineSegement(Vector2 start, Vector2 end)
    {
        Debug.Assert(start != end, $"{start} == {end}");

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;

        return Vector2.Normalize(new Vector2(-dy, dx));
    }    
}
