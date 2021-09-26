using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.FlatShader;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Vortice.Direct3D;

namespace Mini.Engine.Graphics
{
    [Service]
    public partial class ModelSystem : ISystem
    {
        private readonly Device Device;
        private readonly DeferredDeviceContext Context;
        private readonly FrameService FrameService;
        private readonly FlatShaderVs VertexShader;
        private readonly FlatShaderPs PixelShader;
        private readonly InputLayout InputLayout;
        private readonly ConstantBuffer<CBuffer0> ConstantBuffer;


        public ModelSystem(Device device, FrameService frameService, ContentManager content)
        {
            this.Device = device;
            this.Context = device.CreateDeferredContextFor<ModelSystem>();
            this.FrameService = frameService;
            this.VertexShader = content.LoadFlatShaderVs();
            this.PixelShader = content.LoadFlatShaderPs();
            this.InputLayout = this.VertexShader.CreateInputLayout(ModelVertex.Elements);
            this.ConstantBuffer = new ConstantBuffer<CBuffer0>(device);
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
            this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);

            this.Context.OM.SetRenderTarget(this.FrameService.GBuffer.Albedo);
            //this.Context.OM.SetRenderTargetToBackBuffer();

            this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
            this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.Default);
        }

        [Process(Query = ProcessQuery.All)]
        public void DrawModel(ModelComponent component)
        {
            var cBuffer = new CBuffer0()
            {
                WorldViewProjection = this.FrameService.Camera.ViewProjection
            };
            this.ConstantBuffer.MapData(this.Context, cBuffer);

            this.Context.IA.SetVertexBuffer(component.Model.Vertices);
            this.Context.IA.SetIndexBuffer(component.Model.Indices);

            this.Context.VS.SetConstantBuffer(CBuffer0.Slot, this.ConstantBuffer);
            // TODO set texture and other shader variables

            for (var i = 0; i < component.Model.PrimitiveCount; i++)
            {
                var primitive = component.Model.Primitives[i];
                this.Context.DrawIndexed(primitive.Count, primitive.Offset, 0);
            }
        }

        public void OnUnSet()
        {
            // TODO: is it really useful to do this asynchronously?
            using var commandList = this.Context.FinishCommandList();
            this.Device.ImmediateContext.ExecuteCommandList(commandList);
        }
    }
}
