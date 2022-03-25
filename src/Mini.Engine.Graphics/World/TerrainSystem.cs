using System;
using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Terrain;
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
    private readonly RenderService RenderService;
    private readonly TerrainVs VertexShader;
    private readonly TerrainPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public TerrainSystem(Device device, FrameService frameService, RenderService renderService, TerrainVs vertexShader, TerrainPs pixelShader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TerrainSystem>();
        this.FrameService = frameService;
        this.RenderService = renderService;
        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(ModelSystem)}_CB");
    }

    public void OnSet()
    {
        this.Context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.Default);

        this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
        this.Context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
        //this.Context.PS.SetSampler(Terrain.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(TerrainComponent component, TransformComponent transform)
    {
        RenderService.DrawMesh(this, this.Context, this.FrameService.Camera.Frustum, this.FrameService.Camera.ViewProjection, component.Mesh, transform.Transform);
    }

    public void SetConstants(Matrix4x4 worldViewProjection, Matrix4x4 world)
    {
        var cBuffer = new Constants()
        {
            WorldViewProjection = worldViewProjection,
            World = world,
            CameraPosition = this.FrameService.Camera.Transform.Position
        };
        this.ConstantBuffer.MapData(this.Context, cBuffer);
    }
   
    public void OnUnSet()
    {
        // TODO: is it really useful to do this asynchronously?
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.ConstantBuffer.Dispose();
        this.InputLayout.Dispose();
        this.Context.Dispose();
    }
}