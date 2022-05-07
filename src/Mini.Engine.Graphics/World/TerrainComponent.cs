using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.World;

public sealed class TerrainComponent : Component
{
    public TerrainComponent(Entity entity, TerrainMesh terrain)
        : base(entity)
    {
        this.Terrain = terrain;
    }

    public TerrainMesh Terrain { get; set; }
}