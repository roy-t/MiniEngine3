using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Stb = StbImageSharp;
using DXR = Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Content.Textures;

internal sealed class HdrTextureDataLoader : IContentDataLoader<TextureData>
{
    private readonly IVirtualFileSystem FileSystem;

    public HdrTextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSetings ? textureLoaderSetings : TextureLoaderSettings.Default;

        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = Stb.ImageResultFloat.FromStream(stream, Stb.ColorComponents.RedGreenBlueAlpha);
        var format = FormatSelector.SelectHDRFormat(settings.Mode, 4);

        var pitch = image.Width * format.BytesPerPixel();
        
        var imageInfo = new ImageInfo(image.Width, image.Height, format, pitch);
        var mipMapInfo = MipMapInfo.None();
        if (settings.ShouldMipMap)
        {
            mipMapInfo = MipMapInfo.Generated(image.Width);
        }

        var texture = DXR.Textures.Create(id.ToString(), string.Empty, device, imageInfo, mipMapInfo, BindInfo.ShaderResource);
        var view = DXR.ShaderResourceViews.Create(device, texture, imageInfo, id.ToString());

        DXR.Textures.SetPixels<float>(device, texture, view, imageInfo, mipMapInfo, image.Data);

        return new TextureData(id, imageInfo, mipMapInfo, texture, view);
    }
}
