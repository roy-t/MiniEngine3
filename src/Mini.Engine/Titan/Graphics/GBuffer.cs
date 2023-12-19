using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Titan.Graphics;

internal sealed class GBuffer : IDisposable
{
    private readonly Device Device;

    public GBuffer(Device device)
    {
        this.Device = device;
        this.Resize(device.Width, device.Height);
    }

    public IRenderTarget Albedo { get; private set; }
    public DepthStencilBuffer Depth { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float AspectRatio => this.Width / (float)this.Height;

    [MemberNotNull(nameof(Albedo), nameof(Depth))]
    public void Resize(int width, int height)
    {
        this.Dispose();

        var image = new ImageInfo(width, height, Format.R8G8B8A8_UNorm);
        this.Albedo = new RenderTarget(this.Device, nameof(GBuffer) + "Albedo", image, MipMapInfo.None());
        this.Depth = new DepthStencilBuffer(this.Device, nameof(GBuffer) + "Depth", DepthStencilFormat.D32_Float, width, height, 1);
        
        this.Width = width;
        this.Height = height;
    }

    public void Clear()
    {
        this.Device.ImmediateContext.Clear(this.Albedo, Colors.Transparent);
        this.Device.ImmediateContext.Clear(this.Depth, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 0.0f, 0);
    }

    public void Dispose()
    {
        this.Depth?.Dispose();
        this.Albedo?.Dispose();
    }
}
