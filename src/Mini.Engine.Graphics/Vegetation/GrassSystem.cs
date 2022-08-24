﻿using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Vortice.Direct3D;
using Vortice.Mathematics;
using Shaders = Mini.Engine.Content.Shaders.Generated;

namespace Mini.Engine.Graphics.Vegetation;

[Service]
public sealed partial class GrassSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly Grass Shader;
    private readonly Grass.User User;

    public GrassSystem(Device device, FrameService frameService, Shaders.Grass shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<GrassSystem>();
        this.FrameService = frameService;
        this.Shader = shader;       
        this.User = shader.CreateUserFor<GrassSystem>();
    }

    public void OnSet()
    {
        this.Context.IA.ClearInputLayout();
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

        this.Context.VS.SetShader(this.Shader.Vs);
        this.Context.VS.SetConstantBuffer(Grass.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.RS.SetRasterizerState(this.Context.Device.RasterizerStates.WireFrame);
        this.Context.RS.SetScissorRect(0, 0, this.Device.Width, this.Device.Height);
        this.Context.RS.SetViewPort(0, 0, this.Device.Width, this.Device.Height);

        this.Context.PS.SetShader(this.Shader.Ps);
        this.Context.PS.SetConstantBuffer(Grass.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.Default);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal);
    }


    float v = 0.0f;

    [Process(Query = ProcessQuery.All)]
    public void DrawGrass(ref GrassComponent grassComponent)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var world = Matrix4x4.Identity;
        var worldViewProjection = world * camera.Camera.GetViewProjection(in cameraTransform.Transform);
        var cameraPosition = cameraTransform.Transform.GetPosition();

        this.v += (1.0f / 220.0f);
        this.v %= 1.0f;

        float tilt = MathF.Sin(EasOutQuad(this.v) * MathF.PI);

        this.User.MapConstants(this.Context, worldViewProjection, world, cameraPosition, tilt);

        this.Context.VS.SetInstanceBuffer(Grass.Instances, grassComponent.InstanceBuffer);
        this.Context.DrawInstanced(7, grassComponent.Instances);
    }

    float EasOutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
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
