using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Lines;

[Service]
public sealed class LineSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly LineRenderService RenderService;
    private readonly FrameService FrameService;
    private readonly IComponentContainer<LineComponent> Lines;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public LineSystem(Device device, LineRenderService renderService, FrameService frameService, IComponentContainer<LineComponent> lines, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<LineSystem>();
        this.RenderService = renderService;
        this.FrameService = frameService;
        this.Lines = lines;
        this.Transforms = transforms;
    }

    public Task<CommandList> Render(Rectangle viewport, Rectangle scissor, float alpha)
    {
        return Task.Run(() =>
        {
            this.RenderService.Setup(this.Context, viewport, scissor);
            this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.LBuffer.Light);

            ref var camera = ref this.FrameService.GetPrimaryCamera();
            ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

            foreach (ref var component in this.Lines.IterateAll())
            {
                var entity = component.Entity;
                if (entity.HasComponent(this.Transforms))
                {
                    ref var line = ref component.Value;
                    ref var transform = ref this.Transforms[component.Entity].Value;

                    this.RenderService.Render(this.Context, in camera.Camera, in cameraTransform.Current, in line, in transform);
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
