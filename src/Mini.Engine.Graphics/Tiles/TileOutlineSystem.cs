﻿using System.Drawing;
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
        this.OnSet(this.Device.Viewport, this.Device.Viewport);
    }

    public void OnSet(in Rectangle viewport, in Rectangle scissor)
    {
        this.RenderService.SetupRenderTileOutline(this.Context, in viewport, in scissor);

        var gBuffer = this.FrameService.GBuffer;
        var lBuffer = this.FrameService.LBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, lBuffer.Light);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTileOutlines(ref TileComponent tile, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        this.RenderService.RenderTileOutline(this.Context, in tile, in transform, in camera, in cameraTransform);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawTileHighlights(ref TileComponent tile, ref TileHighlightComponent highlight, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        this.RenderService.RenderTileHighlight(this.Context, in tile, in highlight, in transform, in camera, in cameraTransform);
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
