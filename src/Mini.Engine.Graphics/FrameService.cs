using System;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Lighting;

namespace Mini.Engine.Graphics;

[Service]
public sealed class FrameService : IDisposable
{
    public FrameService(Device device)
    {
        this.GBuffer = new GeometryBuffer(device);
        this.LBuffer = new LightBuffer(device);

        var transform = Transform.Identity;
        transform.MoveTo(new Vector3(0, 0, 10));
        transform.FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);

        this.Camera = new PerspectiveCamera(this.GBuffer.AspectRatio, transform);
    }

    /// <summary>
    /// How much to interpolate between the previous and current state of any drawables to prevent stutter
    /// </summary>
    public float Alpha { get; set; }

    public GeometryBuffer GBuffer { get; }
    public LightBuffer LBuffer { get; }

    public PerspectiveCamera Camera;

    public void Dispose()
    {
        this.GBuffer.Dispose();
    }
}
