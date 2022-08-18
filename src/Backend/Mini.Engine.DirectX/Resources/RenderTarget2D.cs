using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTarget2D : ITexture2D, Mini.Engine.DirectX.Resources.vNext.IRenderTarget
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
    public int Length => this.ImageInfo.ArraySize;
    public int Levels => this.MipMapInfo.Levels;
    public Format Format => this.ImageInfo.Format;

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    internal ID3D11RenderTargetView ID3D11RenderTargetView { get; }

    ID3D11ShaderResourceView vNext.ISurface.ShaderResourceView
    {
        get => this.ShaderResourceView;
        set { }
    }

    ID3D11Texture2D vNext.ISurface.Texture
    {
        get => this.Texture;
        set { }
    }

    string vNext.ISurface.Name => this.Name;
    Format vNext.ISurface.Format => this.Format;
    int vNext.ISurface.DimX => this.ImageInfo.Width;
    int vNext.ISurface.DimY => this.ImageInfo.Height;
    int vNext.ISurface.DimZ => this.ImageInfo.ArraySize;

    ID3D11RenderTargetView[] vNext.IRenderTarget.ID3D11RenderTargetViews
    {
        get => new[] { this.ID3D11RenderTargetView };
        set { }
    }

    public void Dispose()
    {
        this.ID3D11RenderTargetView.Dispose();
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
