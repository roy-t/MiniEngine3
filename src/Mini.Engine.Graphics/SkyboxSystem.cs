﻿using System.Drawing;
using System.Numerics;
using LibGame.Graphics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;

namespace Mini.Engine.Graphics;

[Service]
public sealed class SkyboxSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;
    private readonly Skybox Shader;
    private readonly Skybox.User User;
    private readonly FrameService FrameService;

    private readonly IComponentContainer<SkyboxComponent> SkyboxContainer;

    private readonly BlendState Opaque;
    private readonly DepthStencilState ReverseZReadOnly;
    private readonly SamplerState LinearClamp;

    public SkyboxSystem(Device device, FrameService frameService, Skybox shader, IComponentContainer<SkyboxComponent> componentContainer)
    {
        this.Context = device.CreateDeferredContextFor<SkyboxSystem>();
        this.CompletionContext = device.ImmediateContext;
        this.Shader = shader;
        this.User = shader.CreateUserFor<SkyboxSystem>();
        this.FrameService = frameService;
        this.SkyboxContainer = componentContainer;

        this.Opaque = device.BlendStates.Opaque;
        this.ReverseZReadOnly = device.DepthStencilStates.ReverseZReadOnly;
        this.LinearClamp = device.SamplerStates.LinearClamp;
    }

    public Task<ICompletable> Render(Rectangle viewport, Rectangle scissor)
    {
        return Task.Run(() =>
        {
            this.Setup(viewport, scissor);

            foreach (ref var component in this.SkyboxContainer.IterateAll())
            {
                this.DrawSkybox(in component.Value);
            }

            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }

    private void Setup(in Rectangle viewport, in Rectangle scissor)
    {
        var blend = this.Opaque;
        var depth = this.ReverseZReadOnly;
        this.Context.SetupFullScreenTriangle(this.Shader.Vs, in viewport, in scissor, this.Shader.Ps, blend, depth);

        this.Context.PS.SetSampler(Skybox.TextureSampler, this.LinearClamp);

        this.Context.OM.SetRenderTargets(this.FrameService.LBuffer.Group, this.FrameService.GBuffer.DepthStencilBuffer);
    }

    private void DrawSkybox(in SkyboxComponent skybox)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;

        var view = Matrix4x4.CreateLookAt(Vector3.Zero, cameraTransform.GetForward(), cameraTransform.GetUp());
        var projection = ProjectionMatrix.InfiniteReversedZ(0.1f, MathF.PI / 2.0f, camera.AspectRatio);
        var worldViewProjection = view * projection;
        Matrix4x4.Invert(worldViewProjection, out var inverse);
        this.User.MapConstants(this.Context, inverse);
        this.Context.VS.SetConstantBuffer(Skybox.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.PS.SetShaderResource(Skybox.CubeMap, skybox.Albedo);
        this.Context.Draw(3);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Context.Dispose();
    }
}
