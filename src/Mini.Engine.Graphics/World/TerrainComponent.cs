using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.World;

public sealed class TerrainComponent : Component
{
    public TerrainComponent(Entity entity, ITexture2D height, ITexture2D normals, IMesh mesh)
        : base(entity)
    {
        this.Height = height;
        this.Normals = normals;
        this.Mesh = mesh;
    }

    public ITexture2D Height { get; }
    public ITexture2D Normals { get; }

    public IMesh Mesh { get; }
}