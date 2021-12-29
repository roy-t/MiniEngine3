using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTarget2D : Texture2D
{
    public RenderTarget2D(Device device, int width, int height, Format format, string name)
        : base(device, width, height, format, false, name)
    {
        this.ID3D11RenderTargetView = device.ID3D11Device.CreateRenderTargetView(this.Texture);
        this.ID3D11RenderTargetView.DebugName = $"{name}_RenderTargetView";
    }

    internal ID3D11RenderTargetView ID3D11RenderTargetView { get; }

    public override void Dispose()
    {
        this.ID3D11RenderTargetView.Dispose();
        base.Dispose();
    }
}
