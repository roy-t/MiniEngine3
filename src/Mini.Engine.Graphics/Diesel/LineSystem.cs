using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Diesel;

[Service]
public sealed class LineSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly LineRenderService RenderService;
    private readonly CameraService CameraService;
    private readonly IComponentContainer<LineComponent> Lines;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public LineSystem(Device device, LineRenderService renderService, CameraService cameraService, IComponentContainer<LineComponent> lines, IComponentContainer<TransformComponent> transforms)
    {
        this.Context = device.CreateDeferredContextFor<LineSystem>();
        this.RenderService = renderService;
        this.CameraService = cameraService;
        this.Lines = lines;
        this.Transforms = transforms;
    }

    public Task<CommandList> DrawLines(RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int height, float alpha)
    {
        return Task.Run(() =>
        {
            this.RenderService.Setup(this.Context, albedo, depth, x, y, width, height);

            ref var camera = ref this.CameraService.GetPrimaryCamera();
            ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();

            foreach (ref var line in this.Lines.IterateAll())
            {
                if (this.Transforms.Contains(line.Entity))
                {                    
                    ref var transform = ref this.Transforms[line.Entity].Value;

                    this.RenderService.Render(this.Context, in camera.Camera, in cameraTransform.Current, in line.Value, in transform);
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
