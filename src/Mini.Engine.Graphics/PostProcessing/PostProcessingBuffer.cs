﻿using System.Numerics;
using LibGame.Mathematics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Cameras;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.PostProcessing;

public sealed class PostProcessingBuffer : IDisposable
{
    private readonly QuasiRandomSequence Sequence;

    public PostProcessingBuffer(Device device)
    {
        this.Sequence = new QuasiRandomSequence(6);

        var colorImageInfo = new ImageInfo(device.Width, device.Height, Format.R16G16B16A16_Float);
        this.PreviousColor = new RenderTarget(device, "ColorBufferB", colorImageInfo, MipMapInfo.None());
        this.CurrentColor = new RenderTarget(device, "ColorBufferA", colorImageInfo, MipMapInfo.None());

        var velocityImageInfo = new ImageInfo(device.Width, device.Height, Format.R16G16_Float);
        this.PreviousVelocity = new RenderTarget(device, "VelocityBufferB", velocityImageInfo, MipMapInfo.None());
        this.CurrentVelocity = new RenderTarget(device, "VelocityBufferA", velocityImageInfo, MipMapInfo.None());

        this.AntiAliasing = AAType.TAA;
    }

    public IRenderTarget PreviousColor { get; private set; }
    public IRenderTarget CurrentColor { get; private set; }

    public IRenderTarget PreviousVelocity { get; private set; }
    public IRenderTarget CurrentVelocity { get; private set; }

    public AAType AntiAliasing { get; set; }

    public void Swap(ref CameraComponent primaryCamera)
    {
        (this.PreviousColor, this.CurrentColor) = (this.CurrentColor, this.PreviousColor);
        (this.PreviousVelocity, this.CurrentVelocity) = (this.CurrentVelocity, this.PreviousVelocity);

        if (this.AntiAliasing == AAType.TAA)
        {
            var w = 2.0f * this.CurrentColor.DimX;
            var h = 2.0f * this.CurrentColor.DimY;

            primaryCamera.PreviousJitter = primaryCamera.Jitter;
            primaryCamera.Jitter = this.Sequence.Next2D(-1.0f / w, 1.0f / w, -1.0f / h, 1.0f / h);
        }
        else
        {

            primaryCamera.PreviousJitter = Vector2.Zero;
            primaryCamera.Jitter = Vector2.Zero;            
        }
    }

    public void Dispose()
    {
        this.CurrentColor.Dispose();
        this.PreviousColor.Dispose();

        this.CurrentVelocity.Dispose();
        this.PreviousVelocity.Dispose();
    }
}
