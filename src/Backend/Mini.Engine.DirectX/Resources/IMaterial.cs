using System;

namespace Mini.Engine.DirectX.Resources;

public interface IMaterial
{
    public ITexture2D Albedo { get; }
    public ITexture2D Metalicness { get; }
    public ITexture2D Normal { get; }
    public ITexture2D Roughness { get; }
    public ITexture2D AmbientOcclusion { get; }
}
