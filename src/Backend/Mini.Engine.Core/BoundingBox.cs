using System.Numerics;

namespace Mini.Engine.Core;

public struct BoundingBox
{
    public BoundingBox(Vector3 min, Vector3 max)
    {
        this.Min = min;
        this.Max = max;
    }

    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public override string ToString()
    {
        return $"Min: {this.Min}, Max: {this.Max}";
    }
}
