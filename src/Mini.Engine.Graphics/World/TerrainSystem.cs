﻿using System.Diagnostics;
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
public sealed partial class TerrainSystem : IMeshRenderServiceCallBack, ISystem, IDisposable
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

        this.InputLayout = this.Shader.CreateInputLayoutForVs(ModelVertex.Elements);
        this.User = terrain.CreateUserFor<TerrainSystem>();
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
        var tint = this.Device.Resources.Get(component.Tint);        

        this.Context.PS.SetShaderResource(Terrain.Normal, normals);
        this.Context.PS.SetShaderResource(Terrain.Albedo, tint);

        var camera = this.FrameService.GetPrimaryCamera().Camera;
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform();        
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);
        
        var viewVolume = new Frustum(viewProjection);
        TerrainRenderService.RenderTerrain(this.Context, in component, in transform, in viewVolume, this);        
    }

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

        this.User.MapConstants(this.Context, previousWorldViewProjection, worldViewProjection, world, cameraTransform.Current.GetPosition(), this.FrameService.PreviousCameraJitter, this.FrameService.CameraJitter);
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