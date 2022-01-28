using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.CubeMapGenerator;
using Mini.Engine.Content.Shaders.EquilateralToAlbedo;
using Mini.Engine.Content.Shaders.EquilateralToIrradiance;
using System.Numerics;
using System;

namespace Mini.Engine.Graphics.Textures.Generators;

[Service]
public sealed class CubeMapGenerator
{
    private const int IrradianceResolution = 32;
    private static readonly CubeMapFace[] Faces = Enum.GetValues<CubeMapFace>();

    private readonly Device Device;
    private readonly CubeMapGeneratorVs VertexShader;
    private readonly EquilateralToAlbedoPs AlbedoPs;
    private readonly EquilateralToIrradiancePs IrradiancePs;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public CubeMapGenerator(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        this.VertexShader = content.LoadCubeMapGeneratorVs();
        this.AlbedoPs = content.LoadEquilateralToAlbedoPs();
        this.IrradiancePs = content.LoadEquilateralToIrradiancePs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(CubeMapGenerator)}_CB");
    }

    public ITextureCube GenerateAlbedo(ITexture2D equirectangular, bool generateMipMaps, string name)
    {
        var resolution = equirectangular.Height / 2;
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, generateMipMaps, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.Setup(this.InputLayout, this.VertexShader, this.AlbedoPs, blend, depth, resolution, resolution);
        context.PS.SetSampler(EquilateralToAlbedo.TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(EquilateralToAlbedo.Texture, equirectangular);

        this.RenderFaces(texture);
        
        return texture;
    }

    public ITextureCube GenerateIrradiance(ITexture2D equirectangular, string name, int resolution = IrradianceResolution)
    {
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, false, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.Setup(this.InputLayout, this.VertexShader, this.AlbedoPs, blend, depth, resolution, resolution);
        context.PS.SetSampler(EquilateralToIrradiance.TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(EquilateralToIrradiance.Texture, equirectangular);

        this.RenderFaces(texture);

        return texture;
    }
    
    private void RenderFaces(RenderTargetCube target)
    {
        var context = this.Device.ImmediateContext;

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, 1.0f, 0.1f, 1.5f);

        for (var i = 0; i < Faces.Length; i++)
        {
            var face = Faces[i];
            var view = GetViewMatrixForFace(face);
            var worldViewProjection = view * projection;
            Matrix4x4.Invert(worldViewProjection, out var inverse);

            var constants = new Constants()
            {
                InverseWorldViewProjection = inverse
            };
            this.ConstantBuffer.MapData(context, constants);
            context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

            context.OM.SetRenderTarget(target, face);
            context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
        }
    }

    private static Matrix4x4 GetViewMatrixForFace(CubeMapFace face)
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
