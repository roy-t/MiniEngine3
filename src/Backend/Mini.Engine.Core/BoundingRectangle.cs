using System.Numerics;
using System.Runtime.InteropServices;

namespace Mini.Engine.Core;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BoundingRectangle
{
    private Vector2 min;
    private Vector2 max;

    public static readonly BoundingRectangle Identity = new(Vector2.One * -0.5f, Vector2.One * 0.5f);

    public BoundingRectangle(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }
}
