﻿using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;
using Vortice.Mathematics;

using Shaders = Mini.Engine.Content.Shaders.Generated;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class BrdfLutGenerator
{
    private const int Resolution = 512;

    private readonly Device Device;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly Shaders.BrdfLutGenerator Shader;

    public BrdfLutGenerator(Device device, FullScreenTriangle fullScreenTriangle, Shaders.BrdfLutGenerator shader)
    {        
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        this.Shader = shader;        
    }

    public ITexture2D Generate()
    {
        var context = this.Device.ImmediateContext;

        var renderTarget = new RenderTarget2D(this.Device, Resolution, Resolution, Format.R16G16_Float, "BrdfLut");

        context.SetupFullScreenTriangle(this.FullScreenTriangle.TextureVs, Resolution, Resolution, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
        context.OM.SetRenderTarget(renderTarget);

        context.Clear(renderTarget, Colors.Black);
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);
        context.Draw(3);

        return renderTarget;
    }
}
