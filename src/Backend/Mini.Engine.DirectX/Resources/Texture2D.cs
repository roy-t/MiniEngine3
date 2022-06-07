using Mini.Engine.Core;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class Texture2D : ITexture2D
{
    public Texture2D(Device device, int width, int height, Format format, bool generateMipMaps, int mipMapSlices, string user, string meaning)
    {
        this.Width = width;
        this.Height = height;
        this.Format = format;

        this.MipMapSlices = generateMipMaps ? Dimensions.MipSlices(width) : mipMapSlices;
        this.Texture = Textures.Create(device, width, height, format, BindFlags.ShaderResource, ResourceOptionFlags.None, 1, this.MipMapSlices, generateMipMaps, user, meaning);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, user, meaning);

        this.Name = DebugNameGenerator.GetName(user, "Texture2D", meaning, format);
    }

    public void SetPixels<T>(Device device, Span<T> pixels, int pitch)
        where T : unmanaged
    {
        device.ID3D11DeviceContext.UpdateSubresource(pixels, this.Texture, 0, pitch, 0);

        if (this.MipMapSlices > 1)
        {
            device.ID3D11DeviceContext.GenerateMips(this.ShaderResourceView);
        }
    }

    public void SetMipMapPixels<T>(Device device, Span<T> pixels, int pitch, int mipMapIndex)
    where T : unmanaged
    {
        device.ID3D11DeviceContext.UpdateSubresource(pixels, this.Texture, mipMapIndex, pitch);
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }
    public int MipMapSlices { get; }

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
