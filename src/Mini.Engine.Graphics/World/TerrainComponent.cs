using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.World;

public struct TerrainComponent : IComponent
{
    public IResource<ITexture2D> Height;
    public IResource<ITexture2D> Normals;
    public IResource<ITexture2D> Tint;
    public IResource<IMesh> Mesh;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}