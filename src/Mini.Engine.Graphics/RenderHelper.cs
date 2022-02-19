using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders.PostProcess;
using Mini.Engine.Content.Shaders.UserInterface;

namespace Mini.Engine.Graphics;

[Service]
public class RenderHelper
{
    private readonly Device Device;
    private readonly PostProcessPs FXAAPixelShader;
    private readonly PostProcessVs FXAAVertexShader;

    private readonly UserInterfaceVs UIVertexShader;
    private readonly UserInterfacePs UIPixelShader;

    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;

    public RenderHelper(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        
        this.FXAAVertexShader = content.LoadPostProcessVs();
        this.FXAAPixelShader = content.LoadPostProcessPs();

        this.UIVertexShader = content.LoadUserInterfaceVs();
        this.UIPixelShader = content.LoadUserInterfacePs();

        this.InputLayout = this.FXAAVertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
    }
    
    public void RenderFXAA(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.Setup(this.InputLayout, this.FXAAVertexShader, this.FXAAPixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None, width, height);
        context.PS.SetSampler(PostProcess.TextureSampler, this.Device.SamplerStates.LinearWrap);

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);
        context.PS.SetShaderResource(PostProcess.Texture, texture);
        context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }

    public void Render(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.Setup(this.InputLayout, this.FXAAVertexShader, this.UIPixelShader, this.Device.BlendStates.NonPreMultiplied, this.Device.DepthStencilStates.None, width, height);
        context.PS.SetSampler(UserInterface.TextureSampler, this.Device.SamplerStates.LinearWrap);

        // TODO: add new shader in psot process.hlsl that better matches because now everything is black?! Why?

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);
        context.PS.SetShaderResource(UserInterface.Texture, texture);
        context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }
}
