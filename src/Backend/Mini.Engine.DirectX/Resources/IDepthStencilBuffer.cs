using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources;
public interface IDepthStencilBufferArray : ITexture2D
{
    internal ID3D11DepthStencilView[] DepthStencilViews { get; }
}
