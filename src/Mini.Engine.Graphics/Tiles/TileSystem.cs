using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using TileShader = Mini.Engine.Content.Shaders.Generated.Tiles;

namespace Mini.Engine.Graphics.Tiles;

[Service]
public sealed partial class TileSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private IComponentContainer<TransformComponent> Transforms;

    private readonly TileShader Shader;
    private readonly TileShader.User User;

    public TileSystem(Device device, FrameService frameService, ContainerStore store, TileShader shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TileSystem>();
        this.FrameService = frameService;
        this.Transforms = store.GetContainer<TransformComponent>();

        this.Shader = shader;
        this.User = shader.CreateUserFor<TileSystem>();
    }

    public void OnSet()
    {
        this.Context.IA.ClearInputLayout();
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
        
        this.Context.VS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.RS.SetRasterizerState(this.Context.Device.RasterizerStates.Default);
        this.Context.RS.SetScissorRect(0, 0, this.Device.Width, this.Device.Height);
        this.Context.RS.SetViewPort(0, 0, this.Device.Width, this.Device.Height);

        this.Context.PS.SetShader(this.Shader.Ps);
        this.Context.PS.SetSampler(TileShader.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
        this.Context.PS.SetConstantBuffer(TileShader.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.ReverseZ);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal, gBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTiles(ref TileComponent tile, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();
        var previousViewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, this.FrameService.PreviousCameraJitter);
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);
        var cameraPosition = cameraTransform.Current.GetPosition();

        
        this.User.MapConstants(this.Context, previousWorld * previousViewProjection, world * viewProjection, world, cameraPosition, this.FrameService.PreviousCameraJitter, this.FrameService.CameraJitter, tile.Columns, tile.Rows);

        var material = this.Device.Resources.Get(tile.Material);

        this.Context.PS.SetShaderResource(TileShader.Albedo, material.Albedo);
        this.Context.PS.SetShaderResource(TileShader.Normal, material.Normal);
        this.Context.PS.SetShaderResource(TileShader.Metalicness, material.Metalicness);
        this.Context.PS.SetShaderResource(TileShader.Roughness, material.Roughness);
        this.Context.PS.SetShaderResource(TileShader.AmbientOcclusion, material.AmbientOcclusion);

        this.Context.VS.SetInstanceBuffer(TileShader.Instances, tile.InstanceBuffer);        

       
        this.Context.VS.SetShader(this.Shader.Vs);
        this.Context.DrawInstanced(4, (int)(tile.Columns * tile.Rows));
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTileBorders(ref TileComponent tile, ref TransformComponent transform)
    {
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.LineStrip);

        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();
        var previousViewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, this.FrameService.PreviousCameraJitter);
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);
        var cameraPosition = cameraTransform.Current.GetPosition();


        this.User.MapConstants(this.Context, previousWorld * previousViewProjection, world * viewProjection, world, cameraPosition, this.FrameService.PreviousCameraJitter, this.FrameService.CameraJitter, tile.Columns, tile.Rows);

        this.Context.VS.SetInstanceBuffer(TileShader.Instances, tile.InstanceBuffer);

        this.Context.VS.SetShader(this.Shader.VsLine);
        this.Context.PS.SetShader(this.Shader.PsLine);
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
