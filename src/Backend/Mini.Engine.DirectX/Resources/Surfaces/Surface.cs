using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public abstract class Surface : ISurface
{
    private bool isDisposed;

    protected ID3D11ShaderResourceView? shaderResourceView;
    protected ID3D11Texture2D? texture;

    protected Surface(string name, ImageInfo image, MipMapInfo mipMapInfo)
    {
        this.Name = name;
        this.ImageInfo = image;
        this.MipMapInfo = mipMapInfo;
    }

    public string Name { get; }    
    public ImageInfo ImageInfo { get; }
    public MipMapInfo MipMapInfo { get; }
    
    public Format Format => this.ImageInfo.Format;
    public int DimX => this.ImageInfo.DimX;
    public int DimY => this.ImageInfo.DimY;
    public int DimZ => this.ImageInfo.DimZ;
    public int MipMapLevels => this.MipMapInfo.Levels;

    public ISurface AsSurface => this;

    ID3D11ShaderResourceView ISurface.ShaderResourceView => this.shaderResourceView!;
    ID3D11Texture2D ISurface.Texture => this.texture!;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed && disposing)
        {
            this.AsSurface.ShaderResourceView.Dispose();
            this.AsSurface.Texture.Dispose();
        }

        this.isDisposed = true;
    }
}
