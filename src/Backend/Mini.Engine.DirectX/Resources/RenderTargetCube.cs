using Mini.Engine.Core;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTargetCube : ITextureCube
{
    public RenderTargetCube(Device device, int resolution, Format format, bool generateMipMaps, string name)
    {
        this.Resolution = resolution;
        this.Format = format;

        this.Texture = Textures.Create(device, resolution, resolution, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.TextureCube, 6, generateMipMaps, name);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, ShaderResourceViewDimension.TextureCube, name);
        
        var mipSlices = Dimensions.MipSlices(resolution);
        this.FaceRenderTargetViews = new ID3D11RenderTargetView[TextureCube.Faces * mipSlices];
        for (var face = 0; face < TextureCube.Faces; face++)
        {            
            for (var slice = 0; slice < mipSlices; slice++)
            {
                var index = Indexes.ToOneDimensional(slice, face, TextureCube.Faces);
                this.FaceRenderTargetViews[index] = RenderTargetViews.Create(device, this.Texture, format, face, slice, name);
            }
        }
    }

    public int Resolution { get; }
    public Format Format { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    internal ID3D11RenderTargetView[] FaceRenderTargetViews { get; }

    public void Dispose()
    {
        for (var i = 0; i < this.FaceRenderTargetViews.Length; i++)
        {
            this.FaceRenderTargetViews[i].Dispose();
        }
        
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
