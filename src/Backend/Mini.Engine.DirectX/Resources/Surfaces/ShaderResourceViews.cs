using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;

internal static class ShaderResourceViews
{
    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, ImageInfo image, string name)
    {
        ShaderResourceViewDescription description;
        if(image.ArraySize == 1)
        {
            description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2D, image.Format);
        }
        else
        {
            description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2DArray, image.Format, 0, -1, 0, image.ArraySize);
        }

        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = name;

        return srv;
    }

    public static ID3D11ShaderResourceView CreateCube(Device device, ID3D11Texture2D texture, ImageInfo image, string name)
    {
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.TextureCube, image.Format);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = name;

        return srv;
    }    
}
