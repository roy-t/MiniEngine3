using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Tiles;

[Service]
public sealed partial class TileWallSystem : ISystem, IDisposable
{ 
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;

    private readonly TileRenderService RenderService;

    public TileWallSystem(Device device, FrameService frameService, TileRenderService renderService)
    {        
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TileWallSystem>();
        this.FrameService = frameService;

        this.RenderService = renderService;
    }

    public void OnSet()
    {
        this.OnSet(this.Device.Viewport, this.Device.Viewport);
    }

    public void OnSet(in Rectangle viewport, in Rectangle scissor)
    {
        this.RenderService.SetupTileWallRender(this.Context, in viewport, in scissor);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal, gBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTiles(ref TileComponent tile, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();
        this.RenderService.RenderTileWalls(this.Context, in tile, in transform, in camera, in cameraTransform);
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
