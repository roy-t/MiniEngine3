using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.World;

public struct TerrainComponent : IComponent
{
    public TerrainMesh Terrain;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}