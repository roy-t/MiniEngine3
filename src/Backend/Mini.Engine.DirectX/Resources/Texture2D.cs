using System;
using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class Texture2D : ITexture2D
{
    public Texture2D(Device device, int width, int height, Format format, bool generateMipMaps, string name)
    {
        this.Dimensions = new Vector2(width, height);
        this.Format = format;

        this.Texture = Textures.Create(device, width, height, format, generateMipMaps, name);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, name);
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

    public Vector2 Dimensions { get; }
    public Format Format { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture2D.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture2D.Texture => this.Texture;

    public void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();        
    }
}
