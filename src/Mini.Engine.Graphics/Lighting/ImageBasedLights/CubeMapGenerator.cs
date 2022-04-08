using System;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Core;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;

using Shaders = Mini.Engine.Content.Shaders.Generated;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class CubeMapGenerator
{
    private const int IrradianceResolution = 32;
    private const int EnvironmentResolution = 512;

    private const int TextureSampler = Shaders.CubeMapGenerator.TextureSampler;
    private const int Texture = Shaders.CubeMapGenerator.Texture;

    private static readonly CubeMapFace[] Faces = Enum.GetValues<CubeMapFace>();

    private readonly Device Device;
    private readonly Shaders.CubeMapGenerator Shader;
    private readonly Shaders.CubeMapGenerator.User User;

    public CubeMapGenerator(Device device, Shaders.CubeMapGenerator shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = shader.CreateUser();
    }

    public ITextureCube GenerateAlbedo(ITexture2D equirectangular, bool generateMipMaps, string name)
    {
        var resolution = equirectangular.Height / 2;
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, generateMipMaps, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.SetupFullScreenTriangle(this.Shader.Vs, resolution, resolution, this.Shader.AlbedoPs, blend, depth);
        context.PS.SetSampler(TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(Texture, equirectangular);

        this.RenderFaces(texture);

        return texture;
    }

    public ITextureCube GenerateIrradiance(ITexture2D equirectangular, string name, int resolution = IrradianceResolution)
    {
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, false, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.SetupFullScreenTriangle(this.Shader.Vs, resolution, resolution, this.Shader.IrradiancePs, blend, depth);
        context.PS.SetSampler(TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(Texture, equirectangular);

        this.RenderFaces(texture);

        return texture;
    }

    public ITextureCube GenerateEnvironment(ITexture2D equirectangular, string name, int resolution = EnvironmentResolution)
    {
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, true, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.SetupFullScreenTriangle(this.Shader.Vs, resolution, resolution, this.Shader.EnvironmentPs, blend, depth);
        context.PS.SetSampler(TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(Texture, equirectangular);

        var mipSlices = Dimensions.MipSlices(resolution);
        for (var slice = 0; slice < mipSlices; slice++)
        {
            var roughness = slice / (mipSlices - 1.0f);

            this.User.MapEnvironmentConstants(context, roughness);

            context.RS.SetViewPort(0, 0, resolution >> slice, resolution >> slice);
            context.PS.SetConstantBuffer(Shaders.CubeMapGenerator.EnvironmentConstantsSlot, this.User.EnvironmentConstantsBuffer);

            this.RenderFaces(texture, slice);
        }


        return texture;
    }

    private void RenderFaces(RenderTargetCube target, int mipSlice = 0)
    {
        var context = this.Device.ImmediateContext;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, 1.0f, 0.1f, 1.5f);

        for (var i = 0; i < Faces.Length; i++)
        {
            var face = Faces[i];
            var view = GetViewMatrixForFace(face);
            var worldViewProjection = view * projection;
            Matrix4x4.Invert(worldViewProjection, out var inverse);
            
            this.User.MapConstants(context, inverse);

            context.VS.SetConstantBuffer(Shaders.CubeMapGenerator.ConstantsSlot, this.User.ConstantsBuffer);

            context.OM.SetRenderTarget(target, face, mipSlice);
            context.Draw(3);
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
