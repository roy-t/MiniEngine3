using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Graphics.World.Vegetation;
public interface IGrassPlacer
{

    public GrassInstanceData[] Place(int count, float min, float max);
}
