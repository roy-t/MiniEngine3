using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Vortice.DXGI;
using Stb = StbImageSharp;

namespace Mini.Engine.Content.Textures;

internal sealed class HdrTextureDataLoader : IContentDataLoader<TextureData>
{
    private const Format HdrFormat = Format.R32G32B32A32_Float;
    private static readonly int FormatSizeInBytes = HdrFormat.BytesPerPixel();

    private readonly IVirtualFileSystem FileSystem;

    public HdrTextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = Stb.ImageResultFloat.FromStream(stream, Stb.ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * FormatSizeInBytes;

        var bytes = new byte[image.Data.Length * sizeof(float)];
        Buffer.BlockCopy(image.Data, 0, bytes, 0, image.Data.Length * sizeof(float));

        var format = HdrFormat;
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSetings ? textureLoaderSetings : TextureLoaderSettings.Default;

        var imageInfo = new ImageInfo(image.Width, image.Height, format, pitch);
        var mipMapInfo = MipMapInfo.None();
        if (settings.ShouldMipMap)
        {
            mipMapInfo = MipMapInfo.Generated(image.Width);
        }

        return new TextureData(id, imageInfo, mipMapInfo, new[] { bytes });
    }
}
