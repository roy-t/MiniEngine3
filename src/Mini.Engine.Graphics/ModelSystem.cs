using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.FlatShader;
using Vortice.Direct3D;
using Mini.Engine.Content;
using System.Numerics;
using Mini.Engine.ECS.Generators.Shared;
using ImGuiNET;

namespace Mini.Engine.Graphics
{
    [Service]
    public partial class ModelSystem : ISystem
    {
        private readonly Device Device;
        private readonly FrameService FrameService;
        private readonly FlatShaderVs VertexShader;
        private readonly FlatShaderPs PixelShader;
        private readonly InputLayout InputLayout;
        private readonly ConstantBuffer<CBuffer0> ConstantBuffer;
        private readonly Model Model;

        private CBuffer0 ShaderConstants;

        public ModelSystem(Device device, FrameService frameService, ContentManager content)
        {
            this.Device = device;
            this.FrameService = frameService;
            this.VertexShader = content.LoadFlatShaderVs();
            this.PixelShader = content.LoadFlatShaderPs();
            this.InputLayout = this.VertexShader.CreateInputLayout(ModelVertex.Elements);
            this.ConstantBuffer = new ConstantBuffer<CBuffer0>(device);
            this.Model = new Model(device, new ModelData());

            this.ShaderConstants = new CBuffer0();
        }


        public void DoAsIm()
        {
            var context = this.Device.ImmediateContext;

            var world = Matrix4x4.Identity;
            var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            var projection = Matrix4x4.CreatePerspectiveFieldOfView
                (
                    (float)(Math.PI / 2),
                    this.FrameService.GBuffer.AspectRatio,
                    0.1f,
                    1000.0f
                );

            var cBuffer = new CBuffer0()
            {
                //WorldViewProjection = world * view * projection
                WorldViewProjection = Matrix4x4.CreateOrthographicOffCenter(0, this.FrameService.GBuffer.Width, this.FrameService.GBuffer.Height, 0, -1.0f, 1.0f)
            };
            this.ConstantBuffer.MapData(context, cBuffer);

            // SetupRenderState
            context.IA.SetInputLayout(this.InputLayout);
            context.IA.SetVertexBuffer(this.Model.Vertices);
            context.IA.SetIndexBuffer(this.Model.Indices);
            context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

            context.VS.SetShader(this.VertexShader);
            context.VS.SetConstantBuffer(CBuffer0.Slot, this.ConstantBuffer);
            context.RS.SetViewPort(0, 0, this.FrameService.GBuffer.Width, this.FrameService.GBuffer.Height);
            context.RS.SetRasterizerState(this.Device.RasterizerStates.CullNone);

            context.PS.SetShader(this.PixelShader);
            context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);

            context.OM.SetRenderTargetToBackBuffer();
            context.OM.SetBlendState(this.Device.BlendStates.AlphaBlend);
            context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

            context.RS.SetScissorRect(0, 0, this.FrameService.GBuffer.Width, this.FrameService.GBuffer.Height);

            context.DrawIndexed(3, 0, 0);
        }


        public void OnSet()
        {
            this.DoAsIm();

            //var context = this.Device.ImmediateContext;

            //context.IA.SetInputLayout(this.InputLayout);
            
            //context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

            //context.VS.SetShader(this.VertexShader);
            
            //context.RS.SetViewPort(0, 0, this.FrameService.GBuffer.Width, this.FrameService.GBuffer.Height);
            //context.RS.SetRasterizerState(this.Device.RasterizerStates.CullNone); // TODO: CCW

            //context.PS.SetShader(this.PixelShader);
            //context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);

            //context.OM.SetRenderTarget(this.FrameService.GBuffer.Albedo);
            ////context.OM.SetRenderTargetToBackBuffer();
            //context.OM.SetBlendState(this.Device.BlendStates.AlphaBlend); // Opaque
            //context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None); // Default

        }

        //[Process(Query = ProcessQuery.All)]
        [Process]
        public void DrawModel()
        {
            //var context = this.Device.ImmediateContext;
            //// TODO: use real camera

            //var world = Matrix4x4.Identity;
            //var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            //var projection = Matrix4x4.CreatePerspectiveFieldOfView
            //    (
            //        (float)(Math.PI / 2),
            //        this.FrameService.GBuffer.AspectRatio,
            //        0.1f,
            //        1000.0f
            //    );

            //this.ShaderConstants = new CBuffer0()
            //{
            //    WorldViewProjection = world * view * projection
            //};
            
            //context.IA.SetVertexBuffer(this.Model.Vertices);
            //context.IA.SetIndexBuffer(this.Model.Indices);

            //this.ConstantBuffer.MapData(this.Device.ImmediateContext, this.ShaderConstants);
            //context.VS.SetConstantBuffer(CBuffer0.Slot, this.ConstantBuffer);
            //// TODO set texture

            //for (var i = 0; i < this.Model.PrimitiveCount; i++)
            //{
            //    var primitive = this.Model.Primitives[i];
            //    context.DrawIndexed(primitive.Count, primitive.Offset, 0);
            //}
        }
    }
}
