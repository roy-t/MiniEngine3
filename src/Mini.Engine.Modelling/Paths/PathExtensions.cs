using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Paths;
public static class PathExtensions
{
    public static Path3D ToPath3D(this Path2D original, float unitZ = 0.0f)
    {
        return new Path3D(original.IsClosed, original.Positions.Select(p => p.ToVector3(unitZ)).ToArray());
    }

    public static Path3D Reverse(this Path3D original)
    {
        var positions = new Vector3[original.Positions.Length];
        Array.Copy(original.Positions, positions, positions.Length);
        Array.Reverse(positions);
        return new Path3D(original.IsClosed, positions);
    }
}
