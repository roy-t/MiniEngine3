using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.World;

public struct TerrainComponent : IComponent
{
    public IResource<IRWTexture> Height;
    public IResource<IRWTexture> Normals;
    public IResource<IRWTexture> Tint;
    public IResource<IMesh> Mesh;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}