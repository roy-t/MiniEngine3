using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.World;

public sealed class TerrainComponent : Component
{
    public TerrainComponent(Entity entity, ITexture2D height, ITexture2D normals, ITexture2D tint, IMesh mesh)
        : base(entity)
    {
        this.Height = height;
        this.Normals = normals;
        this.Tint = tint;
        this.Mesh = mesh;
    }

    public ITexture2D Height { get; }
    public ITexture2D Normals { get; }
    public ITexture2D Tint { get; }

    public IMesh Mesh { get; }
}