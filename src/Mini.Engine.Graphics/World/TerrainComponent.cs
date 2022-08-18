using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.vNext;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.World;

public struct TerrainComponent : IComponent
{
    public IResource<ISurface> Height;
    public IResource<ISurface> Normals;
    public IResource<ISurface> Tint;
    public IResource<IMesh> Mesh;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}