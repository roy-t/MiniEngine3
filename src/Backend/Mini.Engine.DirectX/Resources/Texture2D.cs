using System;
using System.Numerics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public class Texture2D : ITexture2D
{
    public Texture2D(Device device, int width, int height, Format format, bool generateMipMaps = false, string name = "")
    {
        this.Dimensions = new Vector2(width, height);
        this.Format = format;

        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = generateMipMaps ? 0 : 1,
            ArraySize = 1,
            Format = format,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = generateMipMaps ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None
        };

        this.Texture = device.ID3D11Device.CreateTexture2D(description);
        this.Texture.DebugName = name;

        var view = new ShaderResourceViewDescription(this.Texture, ShaderResourceViewDimension.Texture2D, format);
        this.ShaderResourceView = device.ID3D11Device.CreateShaderResourceView(this.Texture, view);
        this.ShaderResourceView.DebugName = $"{name}_SRV";
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

    public virtual void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
