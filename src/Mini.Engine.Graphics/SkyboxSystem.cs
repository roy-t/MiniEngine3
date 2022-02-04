﻿using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Skybox;
using Mini.Engine.ECS.Systems;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Content;
using System.Numerics;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;

namespace Mini.Engine.Graphics;

[Service]
public sealed partial class SkyboxSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly SkyboxVs VertexShader;
    private readonly SkyboxPs PixelShader;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly FrameService FrameService;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public SkyboxSystem(Device device, ContentManager content, CubeMapGenerator cubeMapGenerator, FullScreenTriangle fullScreenTriangle, FrameService frameService)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SkyboxSystem>();
        this.VertexShader = content.LoadSkyboxVs();
        this.PixelShader = content.LoadSkyboxPs();
        this.FullScreenTriangle = fullScreenTriangle;
        this.FrameService = frameService;
        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(SkyboxSystem)}_CB");
    }   

    public void OnSet()
    {
        var width = this.FrameService.GBuffer.Width;
        var height = this.FrameService.GBuffer.Height;
        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.ReadOnly;
        this.Context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, blend, depth, width, height);

        this.Context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        this.Context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);

        this.Context.PS.SetSampler(Skybox.TextureSampler, this.Device.SamplerStates.LinearClamp);        

        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.LBuffer.Light);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawSkybox(SkyboxComponent skybox)
    {
        var camera = this.FrameService.Camera;

        var view = Matrix4x4.CreateLookAt(Vector3.Zero, camera.Transform.Forward, camera.Transform.Up);        
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, camera.AspectRatio, 0.1f, 1.5f);
        var worldViewProjection = view * projection;
        Matrix4x4.Invert(worldViewProjection, out var inverse);

        var constants = new Constants()
        {
            InverseWorldViewProjection = inverse
        };
        this.ConstantBuffer.MapData(this.Context, constants);
        this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

        this.Context.PS.SetShaderResource(Skybox.CubeMap, skybox.Albedo);
        this.Context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.ConstantBuffer.Dispose();
        this.InputLayout.Dispose();
        this.PixelShader.Dispose();
        this.VertexShader.Dispose();
        this.Context.Dispose();
    }
}