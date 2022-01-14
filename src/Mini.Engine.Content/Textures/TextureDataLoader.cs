using Mini.Engine.DirectX;
using Mini.Engine.IO;
using StbImageSharp;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureDataLoader : IContentDataLoader<TextureData>
{
    private const Format ColorFormat = Format.R8G8B8A8_UNorm_SRgb;
    private static readonly int FormatSizeInBytes = ColorFormat.SizeOfInBytes();
    private readonly IVirtualFileSystem FileSystem;

    public TextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings settings)
    {
        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * FormatSizeInBytes;


        var format = ColorFormat;
        if (settings is TextureLoaderSettings textureLoaderSettings)
        {
            format = textureLoaderSettings.PreferredFormat ?? format;
        }

        return new TextureData(id, image.Width, image.Height, pitch, format, image.Data);
    }
}
