using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Skybox;
using Mini.Engine.ECS.Systems;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.DirectX.Contexts;
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
    private readonly FrameService FrameService;    
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public SkyboxSystem(Device device, CubeMapGenerator cubeMapGenerator, FrameService frameService, SkyboxVs vertexShader, SkyboxPs pixelShader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SkyboxSystem>();
        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;
        this.FrameService = frameService;
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(SkyboxSystem)}_CB");
    }   

    public void OnSet()
    {
        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.ReadOnly;
        this.Context.SetupFullScreenTriangle(this.VertexShader, this.PixelShader, blend, depth);

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
        this.Context.Draw(3);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.ConstantBuffer.Dispose();
        this.PixelShader.Dispose();
        this.VertexShader.Dispose();
        this.Context.Dispose();
    }
}
