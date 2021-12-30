using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;

namespace Mini.Engine.Graphics;

[Service]
public sealed class FrameService : IDisposable
{
    public FrameService(Device device)
    {
        this.GBuffer = new GBuffer(device, DepthStencilFormat.D32_Float);
        this.Camera = new PerspectiveCamera(this.GBuffer.AspectRatio, Transform.Identity);
    }

    /// <summary>
    /// How much to interpolate between the previous and current state of any drawables to prevent stutter
    /// </summary>
    public float Alpha { get; set; }

    public GBuffer GBuffer { get; }

    public PerspectiveCamera Camera;

    public void Dispose()
    {
        this.GBuffer.Dispose();
    }
}
