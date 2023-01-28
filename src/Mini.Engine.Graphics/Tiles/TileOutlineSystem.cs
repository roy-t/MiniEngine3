using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;

using TileShader = Mini.Engine.Content.Shaders.Generated.Tiles;

namespace Mini.Engine.Graphics.Tiles;

[Service]
public sealed partial class TileOutlineSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;

    private readonly TileShader Shader;
    private readonly TileShader.User User;

    public TileOutlineSystem(Device device, FrameService frameService, TileShader shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TileSystem>();
        this.FrameService = frameService;

        this.Shader = shader;
        this.User = shader.CreateUserFor<TileSystem>();
    }

    public void OnSet()
    {
        this.Context.IA.ClearInputLayout();
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.LineStrip);

        this.Context.VS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.VS.SetShader(this.Shader.VsLine);

        this.Context.RS.SetRasterizerState(this.Context.Device.RasterizerStates.Default);
        this.Context.RS.SetScissorRect(0, 0, this.Device.Width, this.Device.Height);
        this.Context.RS.SetViewPort(0, 0, this.Device.Width, this.Device.Height);

        this.Context.PS.SetShader(this.Shader.PsLine);
        this.Context.PS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.ReverseZReadOnly);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal, gBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTileOutlines(ref TileComponent tile, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();
        var previousViewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, this.FrameService.PreviousCameraJitter);
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);
        var cameraPosition = cameraTransform.Current.GetPosition();

        this.User.MapConstants(this.Context, previousWorld * previousViewProjection, world * viewProjection, world, cameraPosition, this.FrameService.PreviousCameraJitter, this.FrameService.CameraJitter, tile.Columns, tile.Rows);

        this.Context.VS.SetInstanceBuffer(TileShader.Instances, tile.InstanceBuffer);                
        this.Context.DrawInstanced(5, (int)(tile.Columns * tile.Rows));
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Context.Dispose();
    }
}
