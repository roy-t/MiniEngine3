using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Tiles;

[Service]
public sealed partial class TileOutlineSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;

    private readonly TileRenderService RenderService;


    public TileOutlineSystem(Device device, FrameService frameService, TileRenderService renderService)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TileSystem>();
        this.FrameService = frameService;

        this.RenderService = renderService;
    }

    public void OnSet()
    {
        this.RenderService.SetupTileOutlineRender(this.Context, 0, 0, this.Device.Width, this.Device.Height);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal, gBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTileOutlines(ref TileComponent tile, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        this.RenderService.RenderTileOutline(this.Context, ref tile, ref transform, ref camera, ref cameraTransform);
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
