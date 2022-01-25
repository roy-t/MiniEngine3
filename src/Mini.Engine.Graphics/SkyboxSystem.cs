using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Skybox;
using Mini.Engine.ECS.Systems;
using Mini.Engine.ECS.Generators.Shared;
using Vortice.Direct3D;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Content;
using Mini.Engine.Graphics.Textures.Generators;
using Mini.Engine.DirectX.Resources;
using System.Numerics;

namespace Mini.Engine.Graphics;

[Service]
public partial class SkyboxSystem : ISystem
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly SkyboxVs VertexShader;
    private readonly SkyboxPs PixelShader;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly FrameService FrameService;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    private readonly ITexture2D CubeMap;

    public SkyboxSystem(Device device, ContentManager content, CubeMapGenerator cubeMapGenerator, FullScreenTriangle fullScreenTriangle, FrameService frameService)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SkyboxSystem>();
        this.VertexShader = content.LoadSkyboxVs();
        this.PixelShader = content.LoadSkyboxPs();
        this.FullScreenTriangle = fullScreenTriangle;
        this.FrameService = frameService;
        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, "constants_skyboxsystem");

        var texture = content.LoadTexture(@"Skyboxes\industrial.hdr");
        this.CubeMap = cubeMapGenerator.Generate(texture, false, "CUBECUBE");
    }   

    public void OnSet()
    {
        var width = this.FrameService.GBuffer.Width;
        var height = this.FrameService.GBuffer.Height;

        this.Context.IA.SetInputLayout(this.InputLayout);
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        this.Context.VS.SetShader(this.VertexShader);

        this.Context.RS.SetViewPort(0, 0, width, height);
        this.Context.RS.SetScissorRect(0, 0, width, height);
        this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        this.Context.PS.SetShader(this.PixelShader);
        this.Context.PS.SetSampler(Skybox.TextureSampler, this.Device.SamplerStates.LinearClamp);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.ReadOnly);
        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.LBuffer.Light);

        this.Context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        this.Context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);

        this.Context.PS.SetShaderResource(Skybox.CubeMap, this.CubeMap);        
    }

    [Process(Query = ProcessQuery.None)]
    public void DrawSkybox()
    {
        var camera = this.FrameService.Camera;

        //var view = Matrix4x4.CreateLookAt(Vector3.Zero, camera.Transform.Forward, camera.Transform.Up);
        var view = Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.Zero + camera.Transform.Forward, camera.Transform.Up);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, camera.AspectRatio, 0.1f, 1.5f);
        var worldViewProjection = view * projection;
        Matrix4x4.Invert(worldViewProjection, out var inverse);

        var constants = new Constants()
        {
            InverseWorldViewProjection = inverse
        };
        this.ConstantBuffer.MapData(this.Context, constants);
        this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

        this.Context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }    
}
