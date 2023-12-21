using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;

internal static class ShaderResourceViews
{
    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, string name, ImageInfo image)
    {
        return Create(device, texture, name, image, SamplingInfo.None);
    }

    public static ID3D11ShaderResourceView Create(Device device, ID3D11Texture2D texture, string name, ImageInfo image, SamplingInfo sampling)
    {
        ShaderResourceViewDescription description;
        if (image.DimZ == 1)
        {
            var dimensions = sampling.GetSrvDimensions();
            description = new ShaderResourceViewDescription(texture, dimensions, image.Format);
        }
        else
        {
            var dimensions = sampling.GetSrvDimensions(true);
            description = new ShaderResourceViewDescription(texture, dimensions, image.Format, 0, -1, 0, image.DimZ);
        }

        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = name;

        return srv;
    }

    public static ID3D11ShaderResourceView CreateCube(Device device, ID3D11Texture2D texture, string name, ImageInfo image)
    {
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.TextureCube, image.Format);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = name;

        return srv;
    }
}
