using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.DirectX.Resources.Models;

public interface IMaterial : IDisposable
{
    public ISurface Albedo { get; }
    public ISurface Metalicness { get; }
    public ISurface Normal { get; }
    public ISurface Roughness { get; }
    public ISurface AmbientOcclusion { get; }
}
