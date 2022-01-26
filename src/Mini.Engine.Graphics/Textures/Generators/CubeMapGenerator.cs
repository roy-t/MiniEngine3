using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.EquilateralToCubeMap;
using System;
using System.Numerics;

namespace Mini.Engine.Graphics.Textures.Generators;

[Service]
public class CubeMapGenerator
{
    private readonly Device Device;
    private readonly EquilateralToCubeMapVs VertexShader;
    private readonly EquilateralToCubeMapPs PixelShader;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public CubeMapGenerator(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        this.VertexShader = content.LoadEquilateralToCubeMapVs();
        this.PixelShader = content.LoadEquilateralToCubeMapPs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, "constants_cubemapgenerator");
    }

    public ITexture2D Generate(ITexture2D equirectangular, bool generateMipMaps, string name)
    {
        var resolution = equirectangular.Height / 2;
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, generateMipMaps, name);

        var context = this.Device.ImmediateContext;

        context.IA.SetInputLayout(this.InputLayout);
        context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);

        context.VS.SetShader(this.VertexShader);

        context.RS.SetViewPort(0, 0, resolution, resolution);
        context.RS.SetScissorRect(0, 0, resolution, resolution);
        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        context.PS.SetShader(this.PixelShader);
        context.PS.SetSampler(EquilateralToCubeMap.TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(EquilateralToCubeMap.Texture, equirectangular);

        context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, 1.0f, 0.1f, 1.5f);

        foreach (var face in Enum.GetValues<CubeMapFace>())
        {
            var view = GetViewMatrixForFace(face);
            var worldViewProjection = view * projection;
            Matrix4x4.Invert(worldViewProjection, out var inverse);
            var constants = new Constants()
            {
                InverseWorldViewProjection = inverse
            };
            this.ConstantBuffer.MapData(context, constants);

            context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);            
            context.OM.SetRenderTarget(texture, face);

            context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
        }

        return texture;
    }

    public static Matrix4x4 GetViewMatrixForFace(CubeMapFace face)
    {        
        return face switch
        {
            CubeMapFace.PositiveX => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitX, Vector3.UnitY),
            CubeMapFace.NegativeX => Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitX, Vector3.UnitY),
            CubeMapFace.PositiveY => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ),
            CubeMapFace.NegativeY => Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitY, -Vector3.UnitZ),
            // Invert Z as we assume a left handed (DirectX 9) coordinate system in the source texture
            CubeMapFace.PositiveZ => Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY),
            CubeMapFace.NegativeZ => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY),
            _ => throw new ArgumentOutOfRangeException(nameof(face))
        };
    }
}
