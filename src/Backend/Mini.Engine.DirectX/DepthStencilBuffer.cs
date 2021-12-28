using System;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX;

public enum DepthStencilFormat
{
    D16_UNorm,
    D24_UNorm_S8_UInt,
    D32_Float,
    D32_Float_S8X24_UInt
}

public sealed class DepthStencilBuffer : IDisposable
{
    public DepthStencilBuffer(Device device, DepthStencilFormat depthStencilFormat, int width, int height)
    {
        var format = ToFormat(depthStencilFormat);

        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = format,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None
        };

        this.Texture = device.ID3D11Device.CreateTexture2D(description);
        this.Texture.DebugName = nameof(DepthStencilBuffer);

        this.DepthStencilView = device.ID3D11Device.CreateDepthStencilView(this.Texture);
    }

    internal ID3D11Texture2D Texture { get; }
    internal ID3D11DepthStencilView DepthStencilView { get; }

    private static Format ToFormat(DepthStencilFormat format)
    {
        return format switch
        {
            DepthStencilFormat.D16_UNorm => Format.D16_UNorm,
            DepthStencilFormat.D24_UNorm_S8_UInt => Format.D24_UNorm_S8_UInt,
            DepthStencilFormat.D32_Float => Format.D32_Float,
            DepthStencilFormat.D32_Float_S8X24_UInt => Format.D32_Float_S8X24_UInt,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
    }

    public void Dispose()
    {
        this.DepthStencilView.Dispose();
        this.Texture.Dispose();
    }
}
