using System.Diagnostics;
using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed partial class ModelSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly ModelRenderService ModelRenderService;

    private readonly IComponentContainer<ModelComponent> Models;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public ModelSystem(Device device, FrameService frameService, ModelRenderService modelRenderService, IComponentContainer<ModelComponent> models, IComponentContainer<TransformComponent> transforms)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.FrameService = frameService;
        this.ModelRenderService = modelRenderService;
        this.Models = models;
        this.Transforms = transforms;
    }


    public Task<CommandList> Render(Rectangle viewport, Rectangle scissor, float alpha)
    {
        return Task.Run(() =>
        {
            this.OnSet(viewport, scissor);

            foreach (ref var model in this.Models.IterateAll())
            {
                if (this.Transforms.Contains(model.Entity))
                {
                    ref var transform = ref this.Transforms[model.Entity];
                    this.DrawModel(ref model.Value, ref transform.Value);
                }
            }

            return this.Context.FinishCommandList();
        });
    }

    public void OnSet()
    {
        this.OnSet(this.Device.Viewport, this.Device.Viewport);
    }

    public void OnSet(in Rectangle viewport, in Rectangle scissor)
    {
        this.ModelRenderService.SetupModelRender(this.Context, in viewport, in scissor);
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal, this.FrameService.GBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(ref ModelComponent component, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();
        this.ModelRenderService.RenderModel(this.Context, in component, in transform, in camera, in cameraTransform);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
