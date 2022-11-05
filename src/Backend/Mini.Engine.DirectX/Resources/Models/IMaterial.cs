using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.DirectX.Resources.Models;

public interface IMaterial : IDisposable // TODO: can we get rid of IDisposable somehow?
{
    public string Name { get; }
    public ILifetime<ISurface> Albedo { get; }
    public ILifetime<ISurface> Metalicness { get; }
    public ILifetime<ISurface> Normal { get; }
    public ILifetime<ISurface> Roughness { get; }
    public ILifetime<ISurface> AmbientOcclusion { get; }
}
