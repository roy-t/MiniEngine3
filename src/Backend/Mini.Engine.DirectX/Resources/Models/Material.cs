using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.DirectX.Resources.Models;

public sealed class Material : IMaterial
{
    public Material(string name, ILifetime<ISurface> albedo, ILifetime<ISurface> metalicness, ILifetime<ISurface> normal, ILifetime<ISurface> roughness, ILifetime<ISurface> ambientOcclusion)
    {
        this.Name = name;
        this.Albedo = albedo;
        this.Metalicness = metalicness;
        this.Normal = normal;
        this.Roughness = roughness;
        this.AmbientOcclusion = ambientOcclusion;
    }

    public string Name { get; }
    public ILifetime<ISurface> Albedo { get; }
    public ILifetime<ISurface> Metalicness { get; }
    public ILifetime<ISurface> Normal { get; }
    public ILifetime<ISurface> Roughness { get; }
    public ILifetime<ISurface> AmbientOcclusion { get; }

    public void Dispose()
    {
        // Do nothing
    }
}
