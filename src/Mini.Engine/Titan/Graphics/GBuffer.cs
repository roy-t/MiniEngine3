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
    private readonly MultiSamplingRequest MultiSamplingRequest;

    // TODO: MSAA is more complex for deferred rendering, see: 
    // - https://docs.nvidia.com/gameworks/content/gameworkslibrary/graphicssamples/d3d_samples/antialiaseddeferredrendering.htm
    // - https://www.reddit.com/r/opengl/comments/kvuolj/comment/gj3e5kh/
    public GBuffer(Device device, MultiSamplingRequest multiSamplingRequest)
    {
        this.Device = device;
        this.MultiSamplingRequest = multiSamplingRequest;
        this.Resize(device.Width, device.Height);
    }

    public RenderTarget Albedo { get; private set; }
    public DepthStencilBuffer Depth { get; private set; }
    public RenderTargetGroup Group { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float AspectRatio => this.Width / (float)this.Height;

    [MemberNotNull(nameof(Albedo), nameof(Depth), nameof(Group))]
    public void Resize(int width, int height)
    {
        this.Dispose();

        var image = new ImageInfo(width, height, Format.R8G8B8A8_UNorm);
        this.Albedo = new RenderTarget(this.Device, nameof(GBuffer) + "Albedo", image, MipMapInfo.None(), this.MultiSamplingRequest);
        this.Depth = new DepthStencilBuffer(this.Device, nameof(GBuffer) + "Depth", DepthStencilFormat.D32_Float, width, height, 1, this.MultiSamplingRequest);
        this.Group = new RenderTargetGroup(this.Albedo);


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
        this.Group?.Dispose();
        this.Depth?.Dispose();
        this.Albedo?.Dispose();
    }
}
