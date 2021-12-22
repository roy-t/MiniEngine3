using Mini.Engine.IO;
using StbImageSharp;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

public class TextureDataLoader : IContentDataLoader<TextureData>
{
    private const Format ColorFormat = Format.R8G8B8A8_UNorm_SRgb;
    private static readonly int FormatSizeInBytes = ColorFormat.SizeOfInBytes();
    private readonly IVirtualFileSystem FileSystem;

    public TextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(ContentId id)
    {
        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * FormatSizeInBytes;

        return new TextureData(id, image.Width, image.Height, pitch, ColorFormat, image.Data);
    }
}
