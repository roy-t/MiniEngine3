using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;
public sealed class DepthStencilBufferArray : ITexture2D
{
    public DepthStencilBufferArray(Device device, DepthStencilFormat format, int width, int height, int length, string name)
    {
        this.Width = width;
        this.Height = height;
        this.Length = length;
        this.Format = ToTextureFormat(format);
        this.Name = name;

        this.Texture = Textures.Create(device, width, height, ToTextureFormat(format), BindFlags.DepthStencil | BindFlags.ShaderResource, ResourceOptionFlags.None, length, false, nameof(DepthStencilBuffer));
        this.ShaderResourceView = CreateSRV(device, this.Texture, length, ToShaderResourceViewFormat(format), nameof(DepthStencilBuffer));
        
        this.DepthStencilViews = new ID3D11DepthStencilView[length];
        for (var i = 0; i < length; i++)
        {
            var depthView = new DepthStencilViewDescription(DepthStencilViewDimension.Texture2DArray, ToDepthViewFormat(format), 0, i, 1);
            this.DepthStencilViews[i] = device.ID3D11Device.CreateDepthStencilView(this.Texture, depthView);
            this.DepthStencilViews[i].DebugName = $"{this.Name}_{i}_DSV";
        }        
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int Length { get; }
    public Format Format { get; }
    public int MipMapSlices => 1;

    internal ID3D11Texture2D Texture { get; }
    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11DepthStencilView[] DepthStencilViews { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    private static ID3D11ShaderResourceView CreateSRV(Device device, ID3D11Texture2D texture, int length, Format format, string name)
    {
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2DArray, format, 0, -1, 0, length);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = $"{name}_SRV";

        return srv;
    }

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
        for (var i = 0; i < this.Length; i++)
        {
            this.DepthStencilViews[i].Dispose();
        }
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
