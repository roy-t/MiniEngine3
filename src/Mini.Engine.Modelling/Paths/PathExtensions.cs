using System.Numerics;
using LibGame.Mathematics;

namespace Mini.Engine.Modelling.Paths;
public static class PathExtensions
{
    public static Path3D ToPath3D(this Path2D original, float unitZ = 0.0f)
    {
        return new Path3D(original.IsClosed, original.Positions.Select(p => p.WithZ(unitZ)).ToArray());
    }

    public static Path3D Reverse(this Path3D original)
    {
        var positions = new Vector3[original.Positions.Length];
        Array.Copy(original.Positions, positions, positions.Length);
        Array.Reverse(positions);
        return new Path3D(original.IsClosed, positions);
    }

    public static Path3D Transform(this Path3D original, in Matrix4x4 matrix)
    {
        var positions = new Vector3[original.Positions.Length];
        for (var i = 0; i < original.Positions.Length; i++)
        {
            positions[i] = Vector3.Transform(original.Positions[i], matrix);
        }

        return new Path3D(original.IsClosed, positions);
    }
}
