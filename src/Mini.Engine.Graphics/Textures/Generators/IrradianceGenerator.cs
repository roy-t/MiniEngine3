using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.EquilateralToIrradianceMap;
using System.Numerics;


namespace Mini.Engine.Graphics.Textures.Generators;

[Service]
public sealed class IrradianceGenerator : ICubeMapRenderer
{
    private const int Resolution = 32;

    private readonly Device Device;
    private readonly EquilateralToIrradianceMapVs VertexShader;
    private readonly EquilateralToIrradianceMapPs PixelShader;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public IrradianceGenerator(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        this.VertexShader = content.LoadEquilateralToIrradianceMapVs();
        this.PixelShader = content.LoadEquilateralToIrradianceMapPs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(IrradianceGenerator)}_CB");
    }

    public ITextureCube Generate(ITexture2D equirectangular, string name, int resolution = Resolution)
    {
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, false, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, blend, depth, resolution, resolution);
        context.PS.SetSampler(EquilateralToIrradianceMap.TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(EquilateralToIrradianceMap.Texture, equirectangular);

        CubeMap.RenderFaces(context, this.FullScreenTriangle, texture, this);

        return texture;
    }

    public void SetInverseViewProjection(Matrix4x4 inverse)
    {
        var context = this.Device.ImmediateContext;
        var constants = new Constants()
        {
            InverseWorldViewProjection = inverse
        };
        this.ConstantBuffer.MapData(context, constants);
        context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
    }
}
