using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTarget2D : ITexture2D
{
    public RenderTarget2D(Device device, int width, int height, Format format, string name)        
    {
        this.Dimensions = new Vector2(width, height);
        this.Format = format;

        this.Texture = Textures.Create(device, width, height, format, name);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, name);

        var view = new RenderTargetViewDescription(this.Texture, RenderTargetViewDimension.Texture2D, format);
        this.ID3D11RenderTargetView = device.ID3D11Device.CreateRenderTargetView(this.Texture, view);
        this.ID3D11RenderTargetView.DebugName = $"{name}_RTV";
    }

    public Vector2 Dimensions { get; }
    public Format Format { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture2D.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture2D.Texture => this.Texture;

    internal ID3D11RenderTargetView ID3D11RenderTargetView { get; }

    public void Dispose()
    {
        this.ID3D11RenderTargetView.Dispose();
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
