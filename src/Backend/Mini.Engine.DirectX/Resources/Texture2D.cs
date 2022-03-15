using System;
using Mini.Engine.Core;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class Texture2D : ITexture2D
{
    public Texture2D(Device device, int width, int height, Format format, bool generateMipMaps, string name)
    {
        this.Width = width;
        this.Height = height;
        this.Format = format;

        this.MipMapSlices = generateMipMaps ? Dimensions.MipSlices(width, height) : 1;
        this.Texture = Textures.Create(device, width, height, format, generateMipMaps, name);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, name);

        this.Name = name;
    }

    public Texture2D(Device device, Span<byte> pixels, int width, int height, Format format, bool generateMipMaps = false, string name = "")
        : this(device, width, height, format, generateMipMaps, name)
    {
        if (format.IsCompressed())
        {
            throw new NotSupportedException($"Compressed texture formats are not supported: {format}");
        }

        // Assumes texture is uncompressed and fills the entire buffer
        var pitch = width * format.SizeOfInBytes();
        device.ID3D11DeviceContext.UpdateSubresource(pixels, this.Texture, 0, pitch, 0);

        if (generateMipMaps)
        {
            device.ID3D11DeviceContext.GenerateMips(this.ShaderResourceView);
        }
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
