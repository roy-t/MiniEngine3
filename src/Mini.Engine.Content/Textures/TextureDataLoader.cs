using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Vortice.DXGI;
using Stb = StbImageSharp;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureDataLoader : IContentDataLoader<TextureData>
{
    private const Format ColorFormat = Format.R8G8B8A8_UNorm_SRgb;
    private static readonly int FormatSizeInBytes = ColorFormat.BytesPerPixel();
    private readonly IVirtualFileSystem FileSystem;

    public TextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = Stb.ImageResult.FromStream(stream, Stb.ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * FormatSizeInBytes;

        var format = ColorFormat;
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSettings ? textureLoaderSettings : TextureLoaderSettings.Default;

        if (settings.IsSRgb)
        {
            format = ColorFormat;
        }
        else 
        {
            format = Format.R8G8B8A8_UNorm;
        }

        var imageInfo = new ImageInfo(image.Width, image.Height, format, pitch);
        var mipMapInfo = MipMapInfo.None();
        if (settings.ShouldMipMap)
        {
            mipMapInfo = MipMapInfo.Generated(image.Width);
        }

        return new TextureData(id, imageInfo, mipMapInfo, new[] { image.Data });
    }
}
