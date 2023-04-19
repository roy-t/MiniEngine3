using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Transforms;
using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Lighting;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine.Graphics.Diesel;

[Service]
public sealed class CameraService
{
    private readonly EntityAdministrator Administrator;
    private readonly IComponentContainer<CameraComponent> Cameras;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private Entity cameraEntity;

    public CameraService(EntityAdministrator administrator, IComponentContainer<CameraComponent> cameras, IComponentContainer<TransformComponent> transforms)
    {
        this.Administrator = administrator;
        this.Cameras = cameras;
        this.Transforms = transforms;
    }

    public ref CameraComponent GetPrimaryCamera()
    {
        return ref this.Cameras[this.cameraEntity].Value;
    }

    public ref TransformComponent GetPrimaryCameraTransform()
    {
        return ref this.Transforms[this.cameraEntity].Value;
    }

    public void InitializePrimaryCamera(float width, float height)
    {
        this.cameraEntity = this.Administrator.Create();
        ref var camera = ref this.Cameras.Create(this.cameraEntity);
        camera.Camera = new PerspectiveCamera(0.1f, 250.0f, MathF.PI / 2.0f, width / height);

        ref var transform = ref this.Transforms.Create(this.cameraEntity);
        transform.Current = Transform.Identity
            .SetTranslation(new Vector3(0, 0, 10))
            .FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);
    }

    public void Resize(float width, float height)
    {
        ref var camera = ref this.Cameras[this.cameraEntity].Value;
        camera.Camera = camera.Camera with { AspectRatio = width / height };
    }
}
