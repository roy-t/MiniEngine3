using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public static class Textures
{
    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, string name)
    {
        return Create(device, width, height, format, 1, false, name);
    }

    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, bool generateMipMaps, string name)
    {
        return Create(device, width, height, format, 1, generateMipMaps, name);
    }

    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, int arraySize, bool generateMipMaps, string name)
    {
        return Create(device, width, height, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.None, arraySize, generateMipMaps, name);        
    }

    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, BindFlags bindFlags, ResourceOptionFlags optionFlags, int arraySize, bool generateMipMaps, string name)
    {
        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = generateMipMaps ? 0 : 1,
            ArraySize = arraySize,
            Format = format,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = bindFlags,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = (generateMipMaps ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None) | optionFlags
        };

        var texture = device.ID3D11Device.CreateTexture2D(description);
        texture.DebugName = name;

        return texture;
    }
}
