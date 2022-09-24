using System.Numerics;
using System.Runtime.CompilerServices;
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

    public Vector2 Min
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.min;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.min = value;
    }

    public Vector2 Max
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.max = value;
    }

    public Vector2 Center => (this.min + this.max) / 2.0f;

    public Vector2 Extent => (this.max - this.min) / 2.0f;

    public float Width => this.Extent.X;
    public float Height => this.Extent.Y;
}
