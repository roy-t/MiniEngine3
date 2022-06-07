using Mini.Engine.Core;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

// TODO: this is extremely similar to RenderTarget2D
public sealed class RenderTarget2DArray : ITexture2D
{
    public RenderTarget2DArray(Device device, ImageInfo imageInfo, MipMapInfo mipMapInfo, ResourceInfo resourceInfo, string user, string meaning)
    {
        this.ImageInfo = imageInfo;
        this.MipMapInfo = mipMapInfo;

        this.Texture = Textures.Create(user, meaning, device, imageInfo, mipMapInfo, BindInfo.RenderTargetShaderResource, resourceInfo);
        this.Name = DebugNameGenerator.GetName(user, "RT", meaning);

        if (resourceInfo == ResourceInfo.Texture)
        {
            this.ShaderResourceView = CreateSRV(device, this.Texture, imageInfo.ArraySize, imageInfo.Format, user, meaning);
        }
        else
        {
            this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, imageInfo.Format, ShaderResourceViewDimension.TextureCube, user, meaning);
        }

        this.ID3D11RenderTargetViews = new ID3D11RenderTargetView[this.Length * mipMapInfo.Levels];
        for (var i = 0; i < imageInfo.ArraySize; i++)
        {
            for (var s = 0; s < mipMapInfo.Levels; s++)
            {
                var index = Indexes.ToOneDimensional(i, s, imageInfo.ArraySize);
                this.ID3D11RenderTargetViews[index] = RenderTargetViews.Create(device, this.Texture, imageInfo.Format, i, s, user, meaning);
            }
        }
    }

    private static ID3D11ShaderResourceView CreateSRV(Device device, ID3D11Texture2D texture, int length, Format format, string user, string meaning)
    {
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2DArray, format, 0, -1, 0, length);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = DebugNameGenerator.GetName(user, "SRV", meaning);

        return srv;
    }

    public string Name { get; }

    public ImageInfo ImageInfo { get; }
    public MipMapInfo MipMapInfo { get; }
    public ResourceInfo ResourceInfo { get; }

    public int Width => this.ImageInfo.Width;
    public int Height => this.ImageInfo.Height;
    public int Length => this.ImageInfo.ArraySize;
    public int Levels => this.MipMapInfo.Levels;
    public Format Format => this.ImageInfo.Format;
    
    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    internal ID3D11RenderTargetView[] ID3D11RenderTargetViews { get; }

    public void Dispose()
    {
        for (var i = 0; i < this.ID3D11RenderTargetViews.Length; i++)
        {
            this.ID3D11RenderTargetViews[i].Dispose();
        }

        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
