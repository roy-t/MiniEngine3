using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTargetCube : ITexture2D
{
    public RenderTargetCube(Device device, int resolution, Format format, bool generateMipMaps, string name)
    {
        this.Width = resolution;
        this.Height = resolution;
        this.Format = format;

        this.Texture = Textures.Create(device, resolution, resolution, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.TextureCube, 6, generateMipMaps, name);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, ShaderResourceViewDimension.TextureCube, name);

        this.FaceRenderTargetViews = new ID3D11RenderTargetView[TextureCube.Faces];
        for (var i = 0; i < TextureCube.Faces; i++)
        {
            this.FaceRenderTargetViews[i] = RenderTargetViews.Create(device, this.Texture, format, i, name);
        }
    }

    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture2D.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture2D.Texture => this.Texture;

    internal ID3D11RenderTargetView[] FaceRenderTargetViews { get; }

    public void Dispose()
    {
        for (var i = 0; i < TextureCube.Faces; i++)
        {
            this.FaceRenderTargetViews[i].Dispose();
        }
        
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
