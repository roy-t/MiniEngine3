using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public abstract class Surface : ISurface
{
    private bool isDisposed;

    protected Surface(string name, ImageInfo image)
    {
        this.Name = name;
        this.ImageInfo = image;
    }

    protected void SetResources(ID3D11Texture2D texture, ID3D11ShaderResourceView view)
    {
        (this as ISurface).Texture = texture;
        (this as ISurface).ShaderResourceView = view;
    }

    public string Name { get; }
    public ImageInfo ImageInfo { get; }

    public Format Format => this.ImageInfo.Format;
    public int DimX => this.ImageInfo.DimX;
    public int DimY => this.ImageInfo.DimY;
    public int DimZ => this.ImageInfo.DimZ;

    public ISurface AsSurface => this;

#nullable disable
    ID3D11ShaderResourceView ISurface.ShaderResourceView { get; set; }
    ID3D11Texture2D ISurface.Texture { get; set; }
#nullable restore

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
