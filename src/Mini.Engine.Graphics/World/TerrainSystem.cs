using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed partial class TerrainSystem : IMeshRenderCallBack, ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly Terrain Shader;
    private readonly Terrain.User User;
    private readonly InputLayout InputLayout;    

    public TerrainSystem(Device device, FrameService frameService, Terrain terrain)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TerrainSystem>();
        this.FrameService = frameService;
        this.Shader = terrain;

        this.InputLayout = this.Shader.Vs.CreateInputLayout(device, ModelVertex.Elements);
        this.User = terrain.CreateUserFor<TerrainSystem>();
    }

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, this.Shader.Vs, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.Default);

        this.Context.VS.SetConstantBuffer(Terrain.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetConstantBuffer(Terrain.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetSampler(Terrain.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);        
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(TerrainComponent component, TransformComponent transform)
    {
        this.Context.PS.SetShaderResource(Terrain.Normal, component.Normals);
        this.Context.PS.SetShaderResource(Terrain.Albedo, component.Tint);
        RenderService.DrawMesh(this, this.Context, this.FrameService.Camera.Frustum, this.FrameService.Camera.ViewProjection, component.Mesh, transform.Transform);
    }

    public void SetConstants(Matrix4x4 worldViewProjection, Matrix4x4 world)
    {
        this.User.MapConstants(this.Context, worldViewProjection, world, this.FrameService.Camera.Transform.Position);
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