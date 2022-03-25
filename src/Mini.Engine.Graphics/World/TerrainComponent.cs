using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.World;

public sealed class TerrainComponent : Component
{
    public TerrainComponent(Entity entity, ITexture2D heightMap, IMesh mesh)
        : base(entity)
    {
        this.HeightMap = heightMap;
        this.Mesh = mesh;
    }

    public ITexture2D HeightMap { get; }
    public IMesh Mesh { get; }
}