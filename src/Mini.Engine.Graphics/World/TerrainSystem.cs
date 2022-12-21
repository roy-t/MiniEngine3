using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed partial class TerrainSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly Terrain Shader;
    private readonly Terrain.User User;
    private readonly InputLayout InputLayout;
    private readonly InternalRenderServiceCallBack CallBack;

    public TerrainSystem(Device device, FrameService frameService, Terrain terrain)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TerrainSystem>();
        this.FrameService = frameService;
        this.Shader = terrain;

        this.InputLayout = this.Shader.CreateInputLayoutForVs(ModelVertex.Elements);
        this.User = terrain.CreateUserFor<TerrainSystem>();

        this.CallBack = new InternalRenderServiceCallBack(frameService, this.Context, this.User);
    }

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, this.Shader.Vs, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.ReverseZ);

        this.Context.VS.SetConstantBuffer(Terrain.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetConstantBuffer(Terrain.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetSampler(Terrain.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
        
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal, this.FrameService.GBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(ref TerrainComponent component, ref TransformComponent transform)
    {
        var normals = this.Device.Resources.Get(component.Normals);
        var erosion = this.Device.Resources.Get(component.Erosion);        

        this.Context.PS.SetShaderResource(Terrain.HeigthMapNormal, normals);
        this.Context.PS.SetShaderResource(Terrain.Erosion, erosion);

        var material = this.Device.Resources.Get(component.Material);
        this.Context.PS.SetShaderResource(Terrain.Albedo, material.Albedo);
        this.Context.PS.SetShaderResource(Terrain.Normal, material.Normal);
        this.Context.PS.SetShaderResource(Terrain.Metalicness, material.Metalicness);
        this.Context.PS.SetShaderResource(Terrain.Roughness, material.Roughness);
        this.Context.PS.SetShaderResource(Terrain.AmbientOcclusion, material.AmbientOcclusion);

        var camera = this.FrameService.GetPrimaryCamera().Camera;
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform();        
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);
        
        var viewVolume = new Frustum(viewProjection);

        this.CallBack.ErosionColor = component.ErosionColor;
        this.CallBack.DepositionColor = component.DepositionColor;
        this.CallBack.ErosionColorMultiplier = component.ErosionColorMultiplier;
        TerrainRenderService.RenderTerrain(this.Context, in component, in transform, in viewVolume, this.CallBack);        
    }

    public void OnUnSet()
    {
        // TODO: is it really useful to do this asynchronously?
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
        this.Context.Dispose();
    }

    private sealed class InternalRenderServiceCallBack : IMeshRenderServiceCallBack
    {
        private readonly FrameService FrameService;
        private readonly DeviceContext Context;
        private readonly Terrain.User User;

        public InternalRenderServiceCallBack(FrameService frameService, DeviceContext context, Terrain.User user)
        {
            this.FrameService = frameService;
            this.Context = context;
            this.User = user;
        }

        public Vector3 DepositionColor { get; set; }
        public Vector3 ErosionColor { get; set; }
        public float ErosionColorMultiplier { get; set; }

        public void RenderMesh(in TransformComponent transform)
        {
            var camera = this.FrameService.GetPrimaryCamera().Camera;
            var cameraTransform = this.FrameService.GetPrimaryCameraTransform();
            var previousViewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, this.FrameService.PreviousCameraJitter);
            var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);

            var previousWorld = transform.Previous.GetMatrix();
            var world = transform.Current.GetMatrix();

            var previousWorldViewProjection = previousWorld * previousViewProjection;
            var worldViewProjection = world * viewProjection;

            this.User.MapConstants(this.Context, previousWorldViewProjection, worldViewProjection, world, cameraTransform.Current.GetPosition(), this.FrameService.PreviousCameraJitter, this.FrameService.CameraJitter, this.DepositionColor, this.ErosionColor, this.ErosionColorMultiplier);
        }
    }
}