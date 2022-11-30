﻿using System;
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
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed partial class ModelSystem : IModelRenderCallBack, ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly RenderService RenderService;
    private readonly Geometry Shader;
    private readonly Geometry.User User;
    private readonly InputLayout InputLayout;

    public ModelSystem(Device device, FrameService frameService, RenderService renderService, Geometry shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.FrameService = frameService;
        this.RenderService = renderService;
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
        var camera = this.FrameService.GetPrimaryCamera().Camera;
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform().Current;
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform, this.FrameService.CurrentCameraJitter);

        var viewVolume = new Frustum(viewProjection);
        var model = this.Device.Resources.Get(component.Model);
        RenderService.DrawModel(this, this.Context, viewVolume, viewProjection, model, transform.Previous, transform.Current);
    }

    public void SetConstants(Matrix4x4 viewProjection, Matrix4x4 previousWorld, Matrix4x4 world)
    {
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform().Current;

        var jitter = this.FrameService.CurrentCameraJitter + this.FrameService.PreviousCameraJitter;

        var previousWorldViewProjection = previousWorld * viewProjection;
        var worldViewProjection = world * viewProjection;

        this.User.MapConstants(this.Context, previousWorldViewProjection, worldViewProjection, world, jitter, cameraTransform.GetPosition());
    }

    public void SetMaterial(IMaterial material)
    {
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
