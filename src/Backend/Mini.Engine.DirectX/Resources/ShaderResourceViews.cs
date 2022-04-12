using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

internal static class ShaderResourceViews
{
    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, Format format, string user, string meaning)
    {
        return Create(device, texture, format, ShaderResourceViewDimension.Texture2D, user, meaning);
    }

    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, Format format, ShaderResourceViewDimension dimension, string user, string meaning)
    {
        var description = new ShaderResourceViewDescription(texture, dimension, format);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = DebugNameGenerator.GetName(user, "SRV", meaning, format);

        return srv;
    }
}
