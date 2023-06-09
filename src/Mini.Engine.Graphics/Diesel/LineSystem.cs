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
public sealed partial class LineSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly LineRenderService RenderService;
    private readonly FrameService FrameService;
    private readonly IComponentContainer<LineComponent> Lines;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public LineSystem(Device device, LineRenderService renderService, FrameService frameService, IComponentContainer<LineComponent> lines, IComponentContainer<TransformComponent> transforms)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<LineSystem>();
        this.RenderService = renderService;
        this.FrameService = frameService;
        this.Lines = lines;
        this.Transforms = transforms;
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
            this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.LBuffer.Light);

            ref var camera = ref this.FrameService.GetPrimaryCamera();
            ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

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
