using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Diesel;
[Service]
public class PrimitiveSystem
{
    private readonly DeferredDeviceContext Context;
    private readonly CameraService CameraService;
    private readonly IComponentContainer<ModelComponent> Models;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public PrimitiveSystem(Device device, CameraService cameraService,  IComponentContainer<ModelComponent> models, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<PrimitiveSystem>();
        this.CameraService = cameraService;
        this.Models = models;
        this.Transforms = transforms;
    }

    public Task<CommandList> DrawPrimitives()
    {
        return Task<CommandList>.Run(() =>
        {
            // this.Context.Setup()

            ref var camera = ref this.CameraService.GetPrimaryCamera();
            ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();

            var iterator = this.Models.IterateAll();
            while (iterator.MoveNext())
            {
                ref var model = ref iterator.Current;
                if (this.Transforms.Contains(model.Entity))
                {
                    ref var transform = ref this.Transforms[model.Entity].Value;
                    this.DrawPrimitive(in camera, in cameraTransform, in model.Value, in transform);
                }
            }

            return this.Context.FinishCommandList();
        });
    }


    private void DrawPrimitive(in CameraComponent camera, in TransformComponent cameraTransform, in ModelComponent model, in TransformComponent transform)
    {

    }

}
