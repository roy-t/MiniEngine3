using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public class Texture : Surface, ITexture
{
    public Texture(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(name, image, mipMap)
    {
        (var texture, var view) = this.CreateResources(device, image, mipMap, name);

        this.texture = texture;
        this.shaderResourceView = view;
    }

    protected virtual (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, ImageInfo image, MipMapInfo mipMap, string name)
    {
        var texture = Textures.Create(device, name, image, mipMap, BindInfo.ShaderResource, ResourceInfo.Texture);
        var view = ShaderResourceViews.Create(device, texture, name, image);
        return (texture, view);
    }

    public void SetPixels<T>(Device device, ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        Textures.SetPixels(device, this.AsSurface.Texture, this.AsSurface.ShaderResourceView, this.ImageInfo, this.MipMapInfo, pixels);
    }


    public void SetPixels<T>(Device device, ReadOnlySpan<T> pixels, int mipSlice, int arraySlice)
        where T : unmanaged
    {
        Textures.SetPixels(device, this.AsSurface.Texture, this.AsSurface.ShaderResourceView, this.ImageInfo, this.MipMapInfo, pixels, mipSlice, arraySlice);
    }
}
