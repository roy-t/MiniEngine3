using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.World;

public struct TerrainComponent : IComponent
{
    public TerrainMesh Terrain { get; set; }
    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy()
    {        
    }
}