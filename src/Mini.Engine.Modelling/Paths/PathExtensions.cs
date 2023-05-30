using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Paths;
public static class PathExtensions
{
    public static Path3D ToPath3D(this Path2D original, float unitZ = 0.0f)
    {
        return new Path3D(original.IsClosed, original.Positions.Select(p => p.ToVector3(unitZ)).ToArray());
    }
}
