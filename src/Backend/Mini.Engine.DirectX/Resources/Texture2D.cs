using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class Texture2D : ITexture2D
{
    public Texture2D(Device device, ImageInfo imageInfo, MipMapInfo mipMapInfo, string user, string meaning, ResourceInfo resourceInfo = ResourceInfo.Texture)
    {
        this.ImageInfo = imageInfo;
        this.MipMapInfo = mipMapInfo;
        this.Texture = Textures.Create(user, meaning, device, imageInfo, mipMapInfo, BindInfo.ShaderResource, resourceInfo);

        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, this.Format, user, meaning);

        this.Name = DebugNameGenerator.GetName(user, "Texture2D", meaning, this.Format);
    }

    // TODO: replace with static methods from Textures.cs?
    public void SetPixels<T>(Device device, ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        device.ID3D11DeviceContext.UpdateSubresource(pixels, this.Texture, 0, this.ImageInfo.Pitch, 0);

        if (this.MipMapInfo.Flags == MipMapFlags.Generated)
        {
            device.ID3D11DeviceContext.GenerateMips(this.ShaderResourceView);
        }
    }

    public void SetMipMapPixels<T>(Device device, ReadOnlySpan<T> pixels, int mipMapIndex)
        where T : unmanaged
    {
        var pitch = (int)(this.ImageInfo.Pitch / Math.Pow(2, mipMapIndex));
        device.ID3D11DeviceContext.UpdateSubresource(pixels, this.Texture, mipMapIndex, pitch);
    }

    public string Name { get; }

    public ImageInfo ImageInfo { get; }
    public MipMapInfo MipMapInfo { get; }
    
    public int Width => this.ImageInfo.Width;
    public int Height => this.ImageInfo.Height;
    public int Levels => this.MipMapInfo.Levels;
    public int Length => this.ImageInfo.ArraySize;

    public Format Format => this.ImageInfo.Format;

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    public void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
