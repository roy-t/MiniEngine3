using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTarget2D : ITexture2D
{
    public RenderTarget2D(Device device, ImageInfo imageInfo, string user, string meaning)
    {
        this.ImageInfo = imageInfo;
        this.MipMapInfo = MipMapInfo.None();

        this.Texture = Textures.Create(user, meaning, device, imageInfo, this.MipMapInfo, BindInfo.RenderTargetShaderResource);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, imageInfo.Format, user, meaning);
        this.ID3D11RenderTargetView = RenderTargetViews.Create(device, this.Texture, imageInfo.Format, user, meaning);

        this.Name = DebugNameGenerator.GetName(user, "RT", meaning, imageInfo.Format);
    }

    public string Name { get; }

    public ImageInfo ImageInfo { get; }
    public MipMapInfo MipMapInfo { get; }

    public int Width => this.ImageInfo.Width;
    public int Height => this.ImageInfo.Height;
    public int ArraySize => this.ImageInfo.ArraySize;
    public int MipMapSlices => this.MipMapInfo.Levels;
    public Format Format => this.ImageInfo.Format;

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
