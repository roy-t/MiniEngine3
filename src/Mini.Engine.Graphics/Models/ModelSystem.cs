using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
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
    
    public ModelSystem(Device device, FrameService frameService, ModelRenderService modelRenderService)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.FrameService = frameService;
        this.ModelRenderService = modelRenderService;    
    }

    public void OnSet()
    {
        this.ModelRenderService.SetupModelRender(this.Context, 0, 0, this.Device.Width, this.Device.Height);
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
