using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public enum DepthStencilFormat
{
    D16_UNorm,
    D24_UNorm_S8_UInt,
    D32_Float,
    D32_Float_S8X24_UInt
}

public sealed class DepthStencilBuffer : Surface, IDepthStencilBuffer
{
    public DepthStencilBuffer(Device device, string name, DepthStencilFormat format, int dimX, int dimY, int dimZ)
        : base(name, new ImageInfo(dimX, dimY, ToTextureFormat(format), DimZ: dimZ), MipMapInfo.None())
    {
        var image = new ImageInfo(dimX, dimY, ToTextureFormat(format), DimZ: dimZ);
        var texture = Textures.Create(device, name, image, MipMapInfo.None(), BindInfo.DepthStencil);
        var view = CreateSRV(device, texture, image.DimZ, ToShaderResourceViewFormat(format), name, "");

        this.texture = texture;
        this.shaderResourceView = view;

        var dsvs = new ID3D11DepthStencilView[image.DimZ];
        for (var i = 0; i < dsvs.Length; i++)
        {
            var depthView = new DepthStencilViewDescription(DepthStencilViewDimension.Texture2DArray, ToDepthViewFormat(format), 0, i, 1);
            dsvs[i] = device.ID3D11Device.CreateDepthStencilView(texture, depthView);
            dsvs[i].DebugName = DebugNameGenerator.GetName(name, "DSV", ToDepthViewFormat(format), i);
        }

        this.AsDepthStencilBuffer.DepthStencilViews = dsvs;
    }

    public IDepthStencilBuffer AsDepthStencilBuffer => this;

#nullable disable
    ID3D11DepthStencilView[] IDepthStencilBuffer.DepthStencilViews { get; set; }
#nullable restore

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        for (var i = 0; i < this.DimZ; i++)
        {
            this.AsDepthStencilBuffer.DepthStencilViews[i].Dispose();
        }
    }

    private static ID3D11ShaderResourceView CreateSRV(Device device, ID3D11Texture2D texture, int length, Format format, string user, string meaning)
    {
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2DArray, format, 0, -1, 0, length);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = DebugNameGenerator.GetName(user, "SRV", meaning, format);

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
}
