using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.World;

public struct TerrainComponent : IComponent
{
    public ILifetime<ISurface> Height;
    public ILifetime<ISurface> Normals;
    public ILifetime<ISurface> Tint;
    public ILifetime<IMesh> Mesh;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}