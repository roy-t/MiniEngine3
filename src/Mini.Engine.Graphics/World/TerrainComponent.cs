using System.Numerics;
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
    public ILifetime<ISurface> Erosion;
    public ILifetime<IMesh> Mesh;
    public Vector3 ErosionColor;
    public Vector3 DepositionColor;
    public float ErosionColorMultiplier;

    public ILifetime<IMaterial> Material;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }    
}