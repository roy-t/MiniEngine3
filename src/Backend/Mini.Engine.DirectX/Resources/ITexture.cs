using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public interface ITexture : IDeviceResource
{
    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }
    public string Name { get; }
    public Format Format { get; }
    public int Levels { get; }
    public int Length { get; }
}

public interface ITexture2D : ITexture
{
    public int Width { get; }
    public int Height { get; }
}
