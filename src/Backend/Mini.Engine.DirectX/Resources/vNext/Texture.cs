namespace Mini.Engine.DirectX.Resources.vNext;

public sealed class Texture : Surface, ITexture
{
    public Texture(Device device, ImageInfo image, MipMapInfo mipMap, string name)
        : base(name, image)
    {
        var texture = Textures.Create(name, "", device, image, mipMap, BindInfo.ShaderResource, ResourceInfo.Texture);
        var view = ShaderResourceViews.Create(device, texture, image, name);

        this.SetResources(texture, view);
    }

    public MipMapInfo MipMapInfo { get; }

    public int MipMapLevels => this.MipMapInfo.Levels;

    public void SetPixels<T>(Device device, ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        Textures.SetPixels(device, this.AsSurface.Texture, this.AsSurface.ShaderResourceView, this.ImageInfo, this.MipMapInfo, pixels);
    }
}
