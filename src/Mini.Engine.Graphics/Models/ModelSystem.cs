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
public sealed partial class ModelSystem : IModelRenderServiceCallBack, ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;    
    private readonly Geometry Shader;
    private readonly Geometry.User User;
    private readonly InputLayout InputLayout;

    public ModelSystem(Device device, FrameService frameService, Geometry shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.FrameService = frameService;        
        this.Shader = shader;
        this.User = shader.CreateUserFor<ModelSystem>();

        this.InputLayout = this.Shader.CreateInputLayoutForVs(ModelVertex.Elements);
    }

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, this.Shader.Vs, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.ReverseZ);

        this.Context.VS.SetConstantBuffer(Geometry.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetConstantBuffer(Geometry.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetSampler(Geometry.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal, this.FrameService.GBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(ref ModelComponent component, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();        
        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, camera.Jitter);

        var viewVolume = new Frustum(viewProjection);        
        ModelRenderService.RenderModel(this.Context, in component, in transform, in viewVolume, this);
    }

    public void RenderModelCallback(in TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var previousViewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, camera.PreviousJitter);
        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, camera.Jitter);

        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();

        var previousWorldViewProjection = previousWorld * previousViewProjection;
        var worldViewProjection = world * viewProjection;
        
        this.User.MapConstants(this.Context, previousWorldViewProjection, worldViewProjection, world, cameraTransform.Current.GetPosition(), camera.PreviousJitter, camera.Jitter);
    }

    public void RenderPrimitiveCallback(IModel model, Primitive primitive)
    {
        var material = this.Context.Resources.Get(model.Materials[primitive.MaterialIndex]);
        
        this.Context.PS.SetShaderResource(Geometry.Albedo, material.Albedo);
        this.Context.PS.SetShaderResource(Geometry.Normal, material.Normal);
        this.Context.PS.SetShaderResource(Geometry.Metalicness, material.Metalicness);
        this.Context.PS.SetShaderResource(Geometry.Roughness, material.Roughness);
        this.Context.PS.SetShaderResource(Geometry.AmbientOcclusion, material.AmbientOcclusion);
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
}
