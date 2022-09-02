using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using Shaders = Mini.Engine.Content.Shaders.Generated;

namespace Mini.Engine.Graphics.Vegetation;

[Service]
public sealed partial class GrassSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly IComponentContainer<SunLightComponent> SunLights;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly Grass Shader;
    private readonly Grass.User User;

    private float windScrollAccumulator;
    private Vector2 windDirection;

    public GrassSystem(Device device, FrameService frameService, ContainerStore store, Shaders.Grass shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<GrassSystem>();
        this.FrameService = frameService;
        this.SunLights = store.GetContainer<SunLightComponent>();
        this.Transforms = store.GetContainer<TransformComponent>();
        this.Shader = shader;
        this.User = shader.CreateUserFor<GrassSystem>();

        this.windScrollAccumulator = 0.0f;
        this.windDirection = Vector2.Normalize(new Vector2(1.0f, 0.75f));
    }

    public void UpdateWind(float elapsed)
    {
        this.windScrollAccumulator += elapsed;
    }

    public void OnSet()
    {
        this.Context.IA.ClearInputLayout();
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

        this.Context.VS.SetShader(this.Shader.Vs);
        this.Context.VS.SetConstantBuffer(Grass.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.RS.SetRasterizerState(this.Context.Device.RasterizerStates.CullNone);
        this.Context.RS.SetScissorRect(0, 0, this.Device.Width, this.Device.Height);
        this.Context.RS.SetViewPort(0, 0, this.Device.Width, this.Device.Height);

        this.Context.PS.SetShader(this.Shader.Ps);
        this.Context.PS.SetConstantBuffer(Grass.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.Default);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawGrass(ref GrassComponent grassComponent)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var viewProjection = camera.Camera.GetViewProjection(in cameraTransform.Transform);
        var cameraPosition = cameraTransform.Transform.GetPosition();
        var grassToSun = this.GetGrassToSunVector();

        this.User.MapConstants(this.Context, viewProjection, cameraPosition, grassToSun, this.windDirection, this.windScrollAccumulator);

        this.Context.VS.SetInstanceBuffer(Grass.Instances, grassComponent.InstanceBuffer);
        this.Context.DrawInstanced(7, grassComponent.Instances);
    }


    private Vector3 GetGrassToSunVector()
    {
        ref var sun = ref this.SunLights.First(ComponentContainer<SunLightComponent>.AcceptAll);
        ref var transform = ref this.Transforms[sun.Entity];
        return -transform.Transform.GetForward();
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
