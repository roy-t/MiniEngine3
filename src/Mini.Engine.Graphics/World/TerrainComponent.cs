using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.World;

public sealed class TerrainComponent : Component
{
    public TerrainComponent(Entity entity, IMesh mesh)
        : base(entity)
    {
        this.Mesh = mesh;
    }
    
    public IMesh Mesh { get; }
}