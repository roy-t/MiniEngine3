using Mini.Engine.DirectX.Resources.vNext;

namespace Mini.Engine.DirectX.Resources;

public interface IMaterial : IDeviceResource
{
    public ITexture Albedo { get; }
    public ITexture Metalicness { get; }
    public ITexture Normal { get; }
    public ITexture Roughness { get; }
    public ITexture AmbientOcclusion { get; }
}
