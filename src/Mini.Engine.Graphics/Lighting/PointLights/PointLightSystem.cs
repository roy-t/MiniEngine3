using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Models.Generators;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Lighting.PointLights;

[Service]
public sealed partial class PointLightSystem : ISystem, IDisposable
{
    private const float MinimumLightInfluence = 0.001f;

    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly PointLight Shader;
    private readonly PointLight.User User;
    private readonly InputLayout InputLayout;
    private readonly IModel Sphere;

    public PointLightSystem(Device device, ContentManager content, FrameService frameService, PointLight shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<PointLightSystem>();
        this.FrameService = frameService;
        this.Shader = shader;
        this.User = shader.CreateUserFor<PointLightSystem>();
        this.InputLayout = this.Shader.CreateInputLayoutForVs(ModelVertex.Elements);

        var material = content.LoadDefaultMaterial();
        this.Sphere = SphereGenerator.Generate(device, 3, material, "PointLight");
    }

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, this.Shader.Vs, this.Shader.Ps, this.Device.BlendStates.Additive, this.Device.DepthStencilStates.None);
        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(PointLight.TextureSampler, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(PointLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(PointLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(PointLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(PointLight.Material, this.FrameService.GBuffer.Material);

        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;
        
        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform, camera.Jitter);
        Matrix4x4.Invert(viewProjection, out var inverseViewProjection);
        this.User.MapConstants(this.Context, inverseViewProjection, cameraTransform.GetPosition());
        this.Context.PS.SetConstantBuffer(PointLight.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.VS.SetConstantBuffer(PointLight.PerLightConstantsSlot, this.User.PerLightConstantsBuffer);
        this.Context.PS.SetConstantBuffer(PointLight.PerLightConstantsSlot, this.User.PerLightConstantsBuffer);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawPointLight(ref PointLightComponent component, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;

        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform, camera.Jitter);

        var radiusOfInfluence = MathF.Sqrt(component.Strength / MinimumLightInfluence);

        var isInside = Vector3.Distance(cameraTransform.GetPosition(), cameraTransform.GetPosition()) < radiusOfInfluence;
        if (isInside)
        {
            this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwiseNoDepthClip);
        }
        else
        {
            this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwiseNoDepthClip);
        }

        var world = Matrix4x4.CreateScale(radiusOfInfluence) * transform.Current.GetMatrix();

        this.User.MapPerLightConstants(this.Context, world * viewProjection, transform.Current.GetPosition(), component.Strength, component.Color);


        this.Context.IA.SetVertexBuffer(this.Sphere.Vertices);
        this.Context.IA.SetIndexBuffer(this.Sphere.Indices);

        this.Context.DrawIndexed(this.Sphere.Primitives[0].IndexCount, this.Sphere.Primitives[0].IndexOffset, 0);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
        this.Context.Dispose();
        this.Sphere.Dispose();
    }
}
