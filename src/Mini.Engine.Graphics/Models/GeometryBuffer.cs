using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.Models;

public sealed class GeometryBuffer : IDisposable
{
    public GeometryBuffer(Device device)
    {
        var imageInfo = new ImageInfo(device.Width, device.Height, Format.R8G8B8A8_UNorm);
        this.Albedo = new RenderTarget(device, nameof(GeometryBuffer) + "Albedo", imageInfo, MipMapInfo.None());

        imageInfo = new ImageInfo(device.Width, device.Height, Format.R8G8B8A8_UNorm);
        this.Material = new RenderTarget(device, nameof(GeometryBuffer) + "Material", imageInfo, MipMapInfo.None());

        imageInfo = new ImageInfo(device.Width, device.Height, Format.R16G16B16A16_Float);
        this.Normal = new RenderTarget(device, nameof(GeometryBuffer) + "Normal", imageInfo, MipMapInfo.None());

        this.DepthStencilBuffer = new DepthStencilBuffer(device, nameof(GeometryBuffer) + "Depth", DepthStencilFormat.D32_Float, device.Width, device.Height, 1);

        this.Width = device.Width;
        this.Height = device.Height;
    }

    public int Width { get; }
    public int Height { get; }

    public float AspectRatio => this.Width / (float)this.Height;

    public IRenderTarget Albedo { get; }
    public IRenderTarget Material { get; }
    public IRenderTarget Normal { get; }

    public DepthStencilBuffer DepthStencilBuffer { get; }

    public void Dispose()
    {
        this.Albedo.Dispose();
        this.Material.Dispose();
        this.Normal.Dispose();

        this.DepthStencilBuffer.Dispose();
    }
}
