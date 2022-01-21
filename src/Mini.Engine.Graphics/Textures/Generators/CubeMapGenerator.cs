using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.PostProcess;

namespace Mini.Engine.Graphics.Textures.Generators;

[Service]
public class CubeMapGenerator
{
    private readonly Device Device;
    private readonly PostProcessVs VertexShader;
    private readonly PostProcessPs PixelShader;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;

    public CubeMapGenerator(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;

        this.VertexShader = content.LoadPostProcessVs(); // TODO create shader
        this.PixelShader = content.LoadPostProcessPs();

        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
    }

    public ITexture2D Generate(ITexture2D equirectangular, bool generateMipMaps, string name)
    {
        var resolution = equirectangular.Height / 2;

        var cube = new RenderTargetCube(this.Device, resolution, equirectangular.Format, generateMipMaps, name);

        var context = this.Device.ImmediateContext;

        context.IA.SetInputLayout(this.InputLayout);
        context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        context.VS.SetShader(this.VertexShader);

        context.RS.SetViewPort(0, 0, resolution, resolution);
        context.RS.SetScissorRect(0, 0, resolution, resolution);
        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        context.PS.SetShader(this.PixelShader);
        context.PS.SetSampler(PostProcess.TextureSampler, this.Device.SamplerStates.LinearClamp);

        context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);

        context.PS.SetShaderResource(PostProcess.Texture, equirectangular);

        for (var i = 0; i < TextureCube.Faces; i++)
        {
            context.OM.SetRenderTarget(cube, i);

            // TODO: set constant buffer properties and proper shader and draw! See CubeMapGenerator in old project!

            context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
            //this.Device.Clear(cube, i, Vortice.Mathematics.Color4.CornflowerBlue);
        }

        return cube;
    }
}
