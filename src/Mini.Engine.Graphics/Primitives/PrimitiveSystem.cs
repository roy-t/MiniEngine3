using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Primitives;

[Service]
public sealed class PrimitiveSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly PrimitiveRenderService RenderService;
    private readonly FrameService FrameService;

    private readonly IComponentContainer<PrimitiveComponent> Primitives;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<InstancesComponent> Instances;

    public PrimitiveSystem(Device device, PrimitiveRenderService renderService, FrameService frameService, IComponentContainer<PrimitiveComponent> models, IComponentContainer<TransformComponent> transforms, IComponentContainer<InstancesComponent> instances)
    {
        this.Context = device.CreateDeferredContextFor<PrimitiveSystem>();
        this.RenderService = renderService;
        this.FrameService = frameService;
        this.Primitives = models;
        this.Transforms = transforms;
        this.Instances = instances;
    }

    public Task<CommandList> Render(Rectangle viewport, Rectangle scissor, float alpha)
    {
        return Task.Run(() =>
        {
            this.RenderService.Setup(this.Context, viewport, scissor);
            this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal, this.FrameService.GBuffer.Velocity);

            ref var camera = ref this.FrameService.GetPrimaryCamera();
            ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

            foreach (ref var component in this.Primitives.IterateAll())
            {
                var entity = component.Entity;
                if (entity.HasComponents(this.Instances, this.Transforms))
                {
                    ref var primitive = ref component.Value;
                    ref var instances = ref this.Instances[entity].Value;
                    ref var transform = ref this.Transforms[entity].Value;

                    if (instances.InstanceList.Count > 0)
                    {
                        this.RenderService.Render(this.Context, in camera, in cameraTransform, in primitive, in instances, in transform);
                    }
                }
            }

            return this.Context.FinishCommandList();
        });
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
