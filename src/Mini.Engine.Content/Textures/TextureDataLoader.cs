using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using DXR = Mini.Engine.DirectX.Resources;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureDataLoader : IContentDataLoader<TextureData>
{
    private readonly IVirtualFileSystem FileSystem;

    public TextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = Image.FromStream(stream);

        var pitch = image.Width * image.ComponentCount;
        
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSettings ? textureLoaderSettings : TextureLoaderSettings.Default;

        var format = FormatSelector.SelectSDRFormat(settings.Mode, image.ComponentCount);

        var imageInfo = new ImageInfo(image.Width, image.Height, format, pitch);
        var mipMapInfo = MipMapInfo.None();
        if (settings.ShouldMipMap)
        {
            mipMapInfo = MipMapInfo.Generated(image.Width);
        }

        var texture = DXR.Textures.Create(id.ToString(), string.Empty, device, imageInfo, mipMapInfo, BindInfo.ShaderResource);
        var view = DXR.ShaderResourceViews.Create(device, texture, format, id.ToString(), string.Empty);

        DXR.Textures.SetPixels<byte>(device, texture, view, imageInfo, mipMapInfo, image.Data);
        
        return new TextureData(id, imageInfo, mipMapInfo, texture, view);
    }
}
