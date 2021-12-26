using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX;

public class Texture2D : IDisposable
{
    public Texture2D(Device device, int width, int height, Format format, bool generateMipMaps = false, string name = "")
    {
        var levels = 0;
        var options = ResourceOptionFlags.None;

        if (generateMipMaps)
        {
            levels = (int)(1 + Math.Floor(Math.Log2(Math.Max(width, height))));
            options = ResourceOptionFlags.GenerateMips;
        }

        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = levels,
            ArraySize = 1,
            Format = format,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = options
        };

        // TODO: we can remove the view parameter as by default the Shader Resource View can access everything we say here
        //var view = new ShaderResourceViewDescription
        //{
        //    Format = format,
        //    ViewDimension = ShaderResourceViewDimension.Texture2D,
        //    Texture2D = new Texture2DShaderResourceView { MipLevels = -1 }
        //};

        this.Texture = device.ID3D11Device.CreateTexture2D(description);
        this.Texture.DebugName = name;

        this.ShaderResourceView = device.ID3D11Device.CreateShaderResourceView(this.Texture);
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

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    public virtual void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
