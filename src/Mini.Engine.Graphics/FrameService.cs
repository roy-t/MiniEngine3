﻿using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.Graphics.Lighting;
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
        this.Administrator = administrator;
        this.Cameras = cameras;
        this.Transforms = transforms;       
    }

    /// <summary>
    /// How much to interpolate between the previous and current state of any drawables to prevent stutter
    /// </summary>
    public float Alpha { get; set; }

    public float Elapsed { get; set; }

    public GeometryBuffer GBuffer { get; private set; }
    public LightBuffer LBuffer { get; private set; }

    public ref CameraComponent GetPrimaryCamera()
    {
        return ref this.Cameras[this.cameraEntity];
    }

    public ref TransformComponent GetPrimaryCameraTransform()
    {
        return ref this.Transforms[this.cameraEntity];
    }

    public void InitializePrimaryCamera()
    {
        this.cameraEntity = this.Administrator.Create();
        ref var camera = ref this.Cameras.Create(this.cameraEntity);
        camera.Camera = new PerspectiveCamera(0.25f, 250.0f, MathF.PI / 2.0f, this.GBuffer.AspectRatio);

        ref var transform = ref this.Transforms.Create(this.cameraEntity);
        transform.Transform = Transform.Identity
            .SetTranslation(new Vector3(0, 0, 10))
            .FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);
    }

    public void Resize(Device device)
    {
        this.Dispose();

        this.GBuffer = new GeometryBuffer(device);
        this.LBuffer = new LightBuffer(device);

        ref var camera = ref this.Cameras[this.cameraEntity];
        camera.Camera = camera.Camera with { AspectRatio = this.GBuffer.AspectRatio };
    }

    public void Dispose()
    {
        this.GBuffer.Dispose();
        this.LBuffer.Dispose();
    }
}
