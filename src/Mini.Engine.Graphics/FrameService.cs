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

        var transform = StructTransform.Identity;
        transform = transform.SetTranslation(new Vector3(0, 0, 10));
        transform = transform.FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);

        this.Camera = new PerspectiveCamera(this.GBuffer.AspectRatio, transform);
    }

    /// <summary>
    /// How much to interpolate between the previous and current state of any drawables to prevent stutter
    /// </summary>
    public float Alpha { get; set; }

    public GeometryBuffer GBuffer { get; private set; }
    public LightBuffer LBuffer { get; private set; }

    public PerspectiveCamera Camera { get; private set; }

    public void Resize(Device device)
    {
        this.Dispose();

        this.GBuffer = new GeometryBuffer(device);
        this.LBuffer = new LightBuffer(device);

        var transform = this.Camera.Transform;
        this.Camera = new PerspectiveCamera(this.GBuffer.AspectRatio, transform);
    }

    public void Dispose()
    {
        this.GBuffer.Dispose();
        this.LBuffer.Dispose();
    }
}
