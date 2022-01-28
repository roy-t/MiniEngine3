using System;
using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public enum DepthStencilFormat
{
    D16_UNorm,
    D24_UNorm_S8_UInt,
    D32_Float,
    D32_Float_S8X24_UInt
}

public sealed class DepthStencilBuffer : ITexture2D
{
    public DepthStencilBuffer(Device device, DepthStencilFormat format, int width, int height)
    {
        this.Width = width;
        this.Height = height;
        this.Format = ToTextureFormat(format);

        this.Texture = Textures.Create(device, width, height, ToTextureFormat(format), BindFlags.DepthStencil | BindFlags.ShaderResource, ResourceOptionFlags.None, 1, false, nameof(DepthStencilBuffer));
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, ToShaderResourceViewFormat(format), nameof(DepthStencilBuffer));

        var depthView = new DepthStencilViewDescription(DepthStencilViewDimension.Texture2D, ToDepthViewFormat(format));
        this.DepthStencilView = device.ID3D11Device.CreateDepthStencilView(this.Texture, depthView);
        this.DepthStencilView.DebugName = $"{nameof(DepthStencilBuffer)}_DSV";        
    }

    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }
    
    internal ID3D11Texture2D Texture { get; }
    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11DepthStencilView DepthStencilView { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    private static Format ToDepthViewFormat(DepthStencilFormat format)
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

    private static Format ToTextureFormat(DepthStencilFormat format)
    {
        return format switch
        {
            DepthStencilFormat.D16_UNorm => Format.R16_Typeless,
            DepthStencilFormat.D24_UNorm_S8_UInt => Format.R24G8_Typeless,
            DepthStencilFormat.D32_Float => Format.R32_Typeless,
            DepthStencilFormat.D32_Float_S8X24_UInt => Format.R32G8X24_Typeless,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
    }

    private static Format ToShaderResourceViewFormat(DepthStencilFormat format)
    {
        return format switch
        {
            DepthStencilFormat.D16_UNorm => Format.R16_UNorm,
            DepthStencilFormat.D24_UNorm_S8_UInt => Format.R24_UNorm_X8_Typeless,
            DepthStencilFormat.D32_Float => Format.R32_Float,
            DepthStencilFormat.D32_Float_S8X24_UInt => Format.R32_Float_X8X24_Typeless,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
    }

    public void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.DepthStencilView.Dispose();
        this.Texture.Dispose();
    }
}
