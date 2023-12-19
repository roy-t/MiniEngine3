using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Lighting;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics;

[Service]
public sealed class FrameService : IDisposable
{
    private readonly EntityAdministrator Administrator;
    private readonly IComponentContainer<CameraComponent> Cameras;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private Entity cameraEntity;

    public FrameService(Device device, EntityAdministrator administrator, IComponentContainer<CameraComponent> cameras, IComponentContainer<TransformComponent> transforms)
    {
        this.GBuffer = new GeometryBuffer(device);
        this.LBuffer = new LightBuffer(device);
        this.PBuffer = new PostProcessingBuffer(device);
        this.Administrator = administrator;
        this.Cameras = cameras;
        this.Transforms = transforms;       
    }

    /// <summary>
    /// How much to interpolate between the previous and current state of any drawables to prevent stutter
    /// </summary>
    public float Alpha { get; set; }

    /// <summary>
    /// How much simulation time has passed since the last frame
    /// </summary>
    public float ElapsedGameTime { get; set; }

    //public Vector2 CameraJitter => this.PBuffer.Jitter;
    //public Vector2 PreviousCameraJitter => this.PBuffer.PreviousJitter;

    public GeometryBuffer GBuffer { get; private set; }
    public LightBuffer LBuffer { get; private set; }
    public PostProcessingBuffer PBuffer { get; private set; }

    public ref CameraComponent GetPrimaryCamera()
    {
        return ref this.Cameras[this.cameraEntity].Value;
    }

    public ref TransformComponent GetPrimaryCameraTransform()
    {
        return ref this.Transforms[this.cameraEntity].Value;
    }

    public void InitializePrimaryCamera()
    {
        this.cameraEntity = this.Administrator.Create();
        ref var camera = ref this.Cameras.Create(this.cameraEntity);
        camera.Camera = new PerspectiveCamera(0.1f, 250.0f, MathF.PI / 2.0f, this.GBuffer.AspectRatio);

        ref var transform = ref this.Transforms.Create(this.cameraEntity);
        transform.Current = Transform.Identity
            .SetTranslation(new Vector3(0, 0, 10))
            .FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);
    }

    public void Resize(Device device)
    {
        this.Dispose();        
        this.GBuffer = new GeometryBuffer(device);
        this.LBuffer = new LightBuffer(device);
        this.PBuffer = new PostProcessingBuffer(device);

        ref var camera = ref this.Cameras[this.cameraEntity].Value;
        camera.Camera = camera.Camera with { AspectRatio = this.GBuffer.AspectRatio };
    }

    public void Dispose()
    {
        this.GBuffer.Dispose();
        this.LBuffer.Dispose();
        this.PBuffer.Dispose();
    }
}
