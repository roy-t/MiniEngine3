//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Text;
//using System.Threading.Tasks;
//using Mini.Engine.Configuration;
//using Mini.Engine.DirectX;
//using Mini.Engine.DirectX.Buffers;
//using Mini.Engine.DirectX.Contexts;
//using Mini.Engine.DirectX.Resources;
//using Mini.Engine.ECS.Generators.Shared;
//using Vortice.Direct3D;

//namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

//[Service]
//public partial class ImageBasedLightSystem
//{
//    private readonly Device Device;
//    private readonly FullScreenTriangle FullScreenTriangle;
//    private readonly DeferredDeviceContext Context;
//    private readonly FrameService FrameService;
//    private readonly Texture2D BrdfLut;

//    private readonly InputLayout InputLayout;

//    public ImageBasedLightSystem(Device device, FullScreenTriangle fullScreenTriangle, FrameService frameService, BrdfLutGenerator generator)
//    {
//        this.Device = device;
//        this.FullScreenTriangle = fullScreenTriangle;
//        this.Context = device.CreateDeferredContextFor<ImageBasedLightSystem>();
//        this.FrameService = frameService;
//        this.BrdfLut = generator.Generate();
//    }

//    public void OnSet()
//    {
//        var width = this.FrameService.GBuffer.Width;
//        var height = this.FrameService.GBuffer.Height;

//        this.Context.IA.SetInputLayout(this.InputLayout);
//        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

//        this.Context.VS.SetShader(this.VertexShader);

//        this.Context.RS.SetViewPort(0, 0, width, height);
//        this.Context.RS.SetScissorRect(0, 0, width, height);

//        this.Context.PS.SetShader(this.PixelShader);

//        this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearClamp);
//        this.Context.PS.SetShaderResource(PointLight.Albedo, this.FrameService.GBuffer.Albedo);
//        this.Context.PS.SetShaderResource(PointLight.Normal, this.FrameService.GBuffer.Normal);
//        this.Context.PS.SetShaderResource(PointLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
//        this.Context.PS.SetShaderResource(PointLight.Material, this.FrameService.GBuffer.Material);

//        this.Context.PS.SetShaderResource(PointLight.Material, this.FrameService.GBuffer.Material);

//        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

//        this.Context.OM.SetBlendState(this.Device.BlendStates.Additive);
//        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

//        var camera = this.FrameService.Camera;
//        Matrix4x4.Invert(camera.ViewProjection, out var inverseViewProjection);
//        var cBuffer = new Constants()
//        {
//            InverseViewProjection = inverseViewProjection,
//            CameraPosition = camera.Transform.Position
//        };
//        this.ConstantBuffer.MapData(this.Context, cBuffer);
//        this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
//        this.Context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
//    }

//    // TODO: instead of none and making a skybox a property of the frameservice, how about just making it
//    // a component and having the new convention that Entity-1 is a sort of global component?
//    [Process(Query = ProcessQuery.None)]
//    public void Render()
//    {

//    }
//}
