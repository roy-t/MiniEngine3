using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed class ModelSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;
    private readonly FrameService FrameService;
    private readonly ModelRenderService ModelRenderService;

    private readonly IComponentContainer<ModelComponent> Models;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public ModelSystem(Device device, FrameService frameService, ModelRenderService modelRenderService, IComponentContainer<ModelComponent> models, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.CompletionContext = device.ImmediateContext;
        this.FrameService = frameService;
        this.ModelRenderService = modelRenderService;
        this.Models = models;
        this.Transforms = transforms;
    }

    public Task<ICompletable> Render(Rectangle viewport, Rectangle scissor, float alpha)
    {
        return Task.Run(() =>
        {
            this.ModelRenderService.SetupModelRender(this.Context, in viewport, in scissor);
            this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.AlbedoMaterialNormalVelocity, this.FrameService.GBuffer.DepthStencilBuffer);

            foreach (ref var component in this.Models.IterateAll())
            {
                var entity = component.Entity;
                if (entity.HasComponent(this.Transforms))
                {
                    ref var model = ref component.Value;
                    ref var transform = ref this.Transforms[component.Entity].Value;

                    this.DrawModel(in model, in transform);
                }
            }

            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }  
    
    private void DrawModel(in ModelComponent component, in TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();
        this.ModelRenderService.RenderModel(this.Context, in component, in transform, in camera, in cameraTransform);
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
