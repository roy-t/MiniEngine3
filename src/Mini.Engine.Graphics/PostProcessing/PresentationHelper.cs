﻿using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Graphics.PostProcessing;

[Service]
public class PresentationHelper
{
    private readonly Device Device;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly TextureShader TextureShader;
    private readonly ToneMapShader ToneMapShader;

    public PresentationHelper(Device device, FullScreenTriangle fullScreenTriangleShader, TextureShader textureShader, ToneMapShader toneMapShader)
    {
        this.Device = device;

        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.TextureShader = textureShader;
        this.ToneMapShader = toneMapShader;
    }

    public void Present(DeviceContext context, ISurface texture)
    {
        var output = this.Device.Viewport;
        this.Present(context, texture, in output, in output);
    }

    public void PresentMultiSampled(DeviceContext context, ISurface texture)
    {
        var output = this.Device.Viewport;
        this.PresentMultiSampled(context, texture, in output, in output);
    }

    public void Present(DeviceContext context, ISurface texture, in Rectangle viewport, in Rectangle scissor)
    {
        context.OM.SetRenderTargetToBackBuffer();

        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, in viewport, in scissor, this.TextureShader.Ps, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        context.PS.SetSampler(TextureShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        context.PS.SetShaderResource(TextureShader.Texture, texture);
        context.Draw(3);

        context.PS.ClearShaderResource(TextureShader.Texture);
    }

    public void PresentMultiSampled(DeviceContext context, ISurface texture, in Rectangle viewport, in Rectangle scissor)
    {
        context.OM.SetRenderTargetToBackBuffer();

        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, in viewport, in scissor, this.TextureShader.PsmultiSample, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);        
        context.PS.SetShaderResource(TextureShader.TextureMs, texture);
        context.Draw(3);

        context.PS.ClearShaderResource(TextureShader.TextureMs);
    }

    public void ToneMapAndPresent(DeviceContext context, ISurface texture)
    {
        var output = this.Device.Viewport;
        this.ToneMapAndPresent(context, texture, in output, in output);
    }

    public void ToneMapAndPresent(DeviceContext context, ISurface texture, in Rectangle viewport, in Rectangle scissor)
    {
        context.OM.SetRenderTargetToBackBuffer();

        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, in viewport, in scissor, this.ToneMapShader.ToneMap, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        context.PS.SetSampler(ToneMapShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        context.PS.SetShaderResource(ToneMapShader.Texture, texture);
        context.Draw(3);

        context.PS.ClearShaderResource(ToneMapShader.Texture);
    }
}
