﻿using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.EquilateralToCubeMap;
using System.Numerics;

namespace Mini.Engine.Graphics.Textures.Generators;

[Service]
public sealed class CubeMapGenerator : ICubeMapRenderer
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
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(CubeMapGenerator)}_CB");
    }

    public ITextureCube Generate(ITexture2D equirectangular, bool generateMipMaps, string name)
    {
        var resolution = equirectangular.Height / 2;
        var texture = new RenderTargetCube(this.Device, resolution, equirectangular.Format, generateMipMaps, name);

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var context = this.Device.ImmediateContext;
        context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, blend, depth, resolution, resolution);
        context.PS.SetSampler(EquilateralToCubeMap.TextureSampler, this.Device.SamplerStates.LinearClamp);
        context.PS.SetShaderResource(EquilateralToCubeMap.Texture, equirectangular);

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
