﻿using System.Drawing;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed partial class SunLightSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;

    private readonly FullScreenTriangle FullScreenTriangle;

    private readonly SunLight Shader;
    private readonly SunLight.User User;

    private readonly IComponentContainer<SunLightComponent> SunLightContainer;
    private readonly IComponentContainer<CascadedShadowMapComponent> CascadedShadowMapContainer;
    private readonly IComponentContainer<TransformComponent> TransformContainer;

    public SunLightSystem(Device device, FrameService frameService, FullScreenTriangle fullScreenTriangle, SunLight shader, IComponentContainer<SunLightComponent> sunLightContainer, IComponentContainer<CascadedShadowMapComponent> cascadedShadowMapContainer, IComponentContainer<TransformComponent> transformContainer)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SunLightSystem>();
        this.FrameService = frameService;
        this.FullScreenTriangle = fullScreenTriangle;

        this.Shader = shader;
        this.User = shader.CreateUserFor<SunLightSystem>();
        this.SunLightContainer = sunLightContainer;
        this.CascadedShadowMapContainer = cascadedShadowMapContainer;
        this.TransformContainer = transformContainer;
    }

    public Task<CommandList> Render(Rectangle viewport, Rectangle scissor)
    {
        return Task.Run(() =>
        {
            this.OnSet(viewport, scissor);

            foreach (ref var skybox in this.SunLightContainer.IterateAll())
            {
                var entity = skybox.Entity;
                if (this.CascadedShadowMapContainer.Contains(entity) && this.TransformContainer.Contains(entity))
                {
                    ref var shadowMap = ref this.CascadedShadowMapContainer[entity];
                    ref var transform = ref this.TransformContainer[entity];
                    DrawSunLight(ref skybox.Value, ref shadowMap.Value, ref transform.Value);
                }
                
            }

            return this.Context.FinishCommandList();
        });
    }

    public void OnSet()
    {
        this.OnSet(this.Device.Viewport, this.Device.Viewport);
    }

    public void OnSet(in Rectangle viewport, in Rectangle scissor)
    {
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangle.TextureVs, in viewport, in scissor, this.Shader.Ps, this.Device.BlendStates.Additive, this.Device.DepthStencilStates.None);
        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(SunLight.TextureSampler, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(SunLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(SunLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(SunLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(SunLight.Material, this.FrameService.GBuffer.Material);

        this.Context.PS.SetSampler(SunLight.ShadowSampler, this.Device.SamplerStates.CompareLessEqualClamp);

        this.Context.PS.SetConstantBuffer(SunLight.ConstantsSlot, this.User.ConstantsBuffer);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawSunLight(ref SunLightComponent sunlight, ref CascadedShadowMapComponent shadowMap, ref TransformComponent viewPoint)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;        
        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform, camera.Jitter);
        Matrix4x4.Invert(viewProjection, out var inverse);

        var shadow = new SunLight.ShadowProperties()
        {
            Offsets = shadowMap.Offsets,
            Scales = shadowMap.Scales,
            Splits = Pack(shadowMap.Splits),
            ShadowMatrix = shadowMap.GlobalShadowMatrix
        };

        this.User.MapConstants(this.Context, sunlight.Color, -viewPoint.Current.GetForward(), sunlight.Strength, inverse, cameraTransform.GetPosition(), shadow);

        this.Context.PS.SetShaderResource(SunLight.ShadowMap, shadowMap.DepthBuffers);

        this.Context.Draw(3);
    }   

    private static Vector4 Pack(Vector4 vectors)
    {
        return new Vector4(vectors.X , vectors.Y, vectors.Z, vectors.W);
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
