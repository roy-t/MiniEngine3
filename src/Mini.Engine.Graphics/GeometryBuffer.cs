using System;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;

namespace Mini.Engine.Graphics;

public sealed class GeometryBuffer : IDisposable
{
    public GeometryBuffer(Device device)
    {
        this.Albedo = new RenderTarget2D(device, device.Width, device.Height, Format.R8G8B8A8_UNorm, "GBuffer_Albedo");
        this.Material = new RenderTarget2D(device, device.Width, device.Height, Format.R8G8B8A8_UNorm, "GBuffer_Material");
        this.Normal = new RenderTarget2D(device, device.Width, device.Height, Format.R16G16B16A16_Float, "GBuffer_Normal");
        this.DepthStencilBuffer = new DepthStencilBuffer(device, DepthStencilFormat.D32_Float, device.Width, device.Height);

        this.Width = device.Width;
        this.Height = device.Height;
    }

    public int Width { get; }
    public int Height { get; }

    public float AspectRatio => (float)this.Width / (float)this.Height;

    public RenderTarget2D Albedo { get; }
    public RenderTarget2D Material { get; } 
    public RenderTarget2D Normal { get; }

    public DepthStencilBuffer DepthStencilBuffer { get; }

    public void Dispose()
    {
        this.Albedo.Dispose();
        this.Material.Dispose();
        this.Normal.Dispose();

        this.DepthStencilBuffer.Dispose();
    }
}
