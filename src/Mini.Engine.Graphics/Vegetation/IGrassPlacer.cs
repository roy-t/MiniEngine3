using System.Numerics;
using Mini.Engine.Core;
using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Graphics.Vegetation;
public interface IGrassPlacer
{
    public GrassInstanceData[] Place(int count, BoundingRectangle bounds);

    // TODO: use this interface to place grass in GrassGenerator
    // note: that the height will need to be recalculated in the generator because it changes with clumping
    // but we need it here as well to make sure we don't put it on steep terrain
    // We could put heightMap and normalMap in Mini.Engine.Core.Grid (new constructor that takes a T[]) to more easily sample
    // and not have to manually work with stride?
    public GrassInstanceData[] Place(int count, BoundingRectangle bounds, float[] heightMap, Vector3[] normalMap, int stride);
}
