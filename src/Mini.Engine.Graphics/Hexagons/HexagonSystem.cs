﻿using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using Shaders = Mini.Engine.Content.Shaders.Generated;

namespace Mini.Engine.Graphics.Hexagons;

[Service]
public sealed partial class HexagonSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private IComponentContainer<TransformComponent> Transforms;

    private readonly Hexagon Shader;
    private readonly Hexagon.User User;

    public HexagonSystem(Device device, FrameService frameService, ContainerStore store, Shaders.Hexagon shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<HexagonSystem>();
        this.FrameService = frameService;
        this.Transforms = store.GetContainer<TransformComponent>();

        this.Shader = shader;
        this.User = shader.CreateUserFor<HexagonSystem>();
    }

    public void OnSet()
    {
        this.Context.IA.ClearInputLayout();
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        this.Context.VS.SetShader(this.Shader.Vs);
        this.Context.VS.SetConstantBuffer(Hexagon.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.RS.SetRasterizerState(this.Context.Device.RasterizerStates.Default);
        this.Context.RS.SetScissorRect(0, 0, this.Device.Width, this.Device.Height);
        this.Context.RS.SetViewPort(0, 0, this.Device.Width, this.Device.Height);

        this.Context.PS.SetShader(this.Shader.Ps);
        this.Context.PS.SetSampler(Hexagon.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);
        this.Context.PS.SetConstantBuffer(Hexagon.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.ReverseZ);

        var gBuffer = this.FrameService.GBuffer;
        this.Context.OM.SetRenderTargets(gBuffer.DepthStencilBuffer, gBuffer.Albedo, gBuffer.Material, gBuffer.Normal, gBuffer.Velocity);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawHexagons(ref HexagonTerrainComponent hexagons, ref TransformComponent transform)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        var previousWorld = transform.Previous.GetMatrix();
        var world = transform.Current.GetMatrix();
        var previousViewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Previous, this.FrameService.PreviousCameraJitter);
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform.Current, this.FrameService.CameraJitter);
        var cameraPosition = cameraTransform.Current.GetPosition();

        this.User.MapConstants(this.Context, previousWorld * previousViewProjection, world * viewProjection, world, cameraPosition, this.FrameService.PreviousCameraJitter, this.FrameService.CameraJitter);

        var material = this.Device.Resources.Get(hexagons.Material);

        this.Context.PS.SetShaderResource(Hexagon.Albedo, material.Albedo);
        this.Context.PS.SetShaderResource(Hexagon.Normal, material.Normal);
        this.Context.PS.SetShaderResource(Hexagon.Metalicness, material.Metalicness);
        this.Context.PS.SetShaderResource(Hexagon.Roughness, material.Roughness);
        this.Context.PS.SetShaderResource(Hexagon.AmbientOcclusion, material.AmbientOcclusion);

        this.Context.VS.SetInstanceBuffer(Hexagon.Instances, hexagons.InstanceBuffer);

        // 4 triangles for the inside, 6 * 2 triangles for the flaps, 6 * 2 triangles for the flap connections
        var triangleCount = 4 + (6 * 2) + 6;
        var vertexCount = triangleCount * 3;
        this.Context.DrawInstanced(vertexCount, hexagons.Instances);
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