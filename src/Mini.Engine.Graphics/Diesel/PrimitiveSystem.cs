using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Diesel;

[Service]
public sealed partial class PrimitiveSystem : ISystem,  IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly PrimitiveRenderService RenderService;
    private readonly FrameService FrameService;

    private readonly IComponentContainer<PrimitiveComponent> Primitives;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<InstancesComponent> Instances;

    public PrimitiveSystem(Device device, PrimitiveRenderService renderService, FrameService frameService, IComponentContainer<PrimitiveComponent> models, IComponentContainer<TransformComponent> transforms, IComponentContainer<InstancesComponent> instances)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<PrimitiveSystem>();
        this.RenderService = renderService;
        this.FrameService = frameService;
        this.Primitives = models;
        this.Transforms = transforms;
        this.Instances = instances;
    }

    public void OnSet()
    {
        
    }

    public void OnUnSet()
    {
        //
    }

    [Process(Query = ProcessQuery.None)]
    public void Draw()
    {
        //var task = this.Render(0, 0, this.Device.Width, this.Device.Height, this.FrameService.Alpha);
        //task.Wait();

        //var commandList = task.Result;
        //this.Device.ImmediateContext.ExecuteCommandList(commandList);
        //commandList.Dispose();
    }

    public Task<CommandList> Render(Rectangle viewport, float alpha)
    {
        return Task.Run(() =>
        {
            this.RenderService.Setup(this.Context, viewport);
            this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal, this.FrameService.GBuffer.Velocity);

            ref var camera = ref this.FrameService.GetPrimaryCamera();
            ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

            foreach (ref var primitive in this.Primitives.IterateAll())
            {
                if (this.Instances.Contains(primitive.Entity) && this.Transforms.Contains(primitive.Entity))
                {
                    ref var instances = ref this.Instances[primitive.Entity].Value;
                    ref var transform = ref this.Transforms[primitive.Entity].Value;

                    this.RenderService.Render(this.Context, in camera, in cameraTransform, in primitive.Value, in instances, in transform);
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
