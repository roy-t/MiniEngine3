using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Graphics.World;

public sealed class GeneratedTerrain
{
    public GeneratedTerrain(ILifetime<IRWTexture> height, ILifetime<IRWTexture> normals, ILifetime<IRWTexture> erosion, ILifetime<IMesh> mesh)
    {
        this.Height = height;
        this.Normals = normals;
        this.Erosion = erosion;
        this.Mesh = mesh;
    }
    
    public ILifetime<IRWTexture> Height { get; }
    public ILifetime<IRWTexture> Normals { get; }
    public ILifetime<IRWTexture> Erosion { get; }
    public ILifetime<IMesh> Mesh { get; }
}