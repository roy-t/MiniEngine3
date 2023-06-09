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
    private readonly TerrainRenderService TerrainRenderService;

    public TerrainSystem(Device device, FrameService frameService, TerrainRenderService terrainRenderService)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<TerrainSystem>();
        this.FrameService = frameService;
        this.TerrainRenderService = terrainRenderService;
    }

    public void OnSet()
    {
        this.TerrainRenderService.SetupTerrainRender(this.Context, this.Device.Viewport);        
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal, this.FrameService.GBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(ref TerrainComponent component, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();
        this.TerrainRenderService.RenderTerrain(this.Context, in component, in transform, in camera, in cameraTransform);
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