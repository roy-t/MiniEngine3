using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public interface ISurface : IDeviceResource
{
    internal ID3D11ShaderResourceView ShaderResourceView { get; set; }
    internal ID3D11Texture2D Texture { get; set; }

    public string Name { get; }

    public ImageInfo ImageInfo { get; }
    public MipMapInfo MipMapInfo { get; }

    public Format Format { get; }
    public int DimX { get; }
    public int DimY { get; }
    public int DimZ { get; }
}

public interface ITexture : ISurface
{
    public int MipMapLevels { get; }
}

public interface ITextureCube : ISurface
{

}

public interface IRWTexture : ITexture
{
    internal ID3D11UnorderedAccessView[] UnorderedAccessViews { get; set; }
}

public interface IDepthStencilBuffer : ISurface
{
    internal ID3D11DepthStencilView[] DepthStencilViews { get; set; }
}

public interface IRenderTarget : ITexture
{
    internal ID3D11RenderTargetView[] ID3D11RenderTargetViews { get; set; }
}

public interface IRenderTargetCube : IRenderTarget
{

}



