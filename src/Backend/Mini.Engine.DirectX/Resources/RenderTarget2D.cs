using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTarget2D : ITexture2D
{
    public RenderTarget2D(Device device, int width, int height, Format format, string user, string meaning)
    {
        this.Width = width;
        this.Height = height;
        this.Format = format;

        this.Texture = Textures.Create(device, width, height, format, user, meaning);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, user, meaning);
        this.ID3D11RenderTargetView = RenderTargetViews.Create(device, this.Texture, format, user, meaning);

        this.Name = DebugNameGenerator.GetName(user, "RT", meaning, format);
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }
    public int MipMapSlices => 1;

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    internal ID3D11RenderTargetView ID3D11RenderTargetView { get; }

    public void Dispose()
    {
        this.ID3D11RenderTargetView.Dispose();
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
