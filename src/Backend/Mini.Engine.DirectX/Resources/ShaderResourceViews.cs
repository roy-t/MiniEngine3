using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

internal static class ShaderResourceViews
{
    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, Format format, string textureName)
    {
        return Create(device, texture, format, ShaderResourceViewDimension.Texture2D, textureName);
    }

    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, Format format, ShaderResourceViewDimension dimension, string textureName)
    {
        var description = new ShaderResourceViewDescription(texture, dimension, format);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = $"{textureName}_SRV";

        return srv;
    }
}
