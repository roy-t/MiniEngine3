using Mini.Engine.Core;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RenderTargetCube : ITextureCube
{
    public RenderTargetCube(Device device, int resolution, Format format, bool generateMipMaps, string user, string meaning)
    {
        this.Resolution = resolution;
        this.Format = format;

        this.MipMapSlices = generateMipMaps ? Dimensions.MipSlices(resolution) : 1;
        this.Texture = Textures.Create(device, resolution, resolution, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.TextureCube, 6, this.MipMapSlices, generateMipMaps, user, meaning);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, ShaderResourceViewDimension.TextureCube, user, meaning);

        this.MipMapSlices = generateMipMaps ? Dimensions.MipSlices(resolution) : 1;
        this.FaceRenderTargetViews = new ID3D11RenderTargetView[TextureCube.Faces * this.MipMapSlices];
        for (var face = 0; face < TextureCube.Faces; face++)
        {
            for (var slice = 0; slice < this.MipMapSlices; slice++)
            {
                var index = Indexes.ToOneDimensional(face, slice, TextureCube.Faces);
                this.FaceRenderTargetViews[index] = RenderTargetViews.Create(device, this.Texture, format, face, slice, user, meaning);
            }
        }

        this.Name = user;
    }

    public string Name { get; }
    public int Resolution { get; }
    public Format Format { get; }
    public int MipMapSlices { get; }

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
