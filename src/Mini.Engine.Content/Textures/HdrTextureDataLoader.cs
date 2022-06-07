using Mini.Engine.DirectX;
using Mini.Engine.IO;
using StbImageSharp;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed class HdrTextureDataLoader : IContentDataLoader<TextureData>
{
    private const Format HdrFormat = Format.R32G32B32A32_Float;
    private static readonly int FormatSizeInBytes = HdrFormat.SizeOfInBytes();

    private readonly IVirtualFileSystem FileSystem;

    public HdrTextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        using var stream = this.FileSystem.OpenRead(id.Path);
        var image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * FormatSizeInBytes;

        var bytes = new byte[image.Data.Length * sizeof(float)];
        Buffer.BlockCopy(image.Data, 0, bytes, 0, image.Data.Length * sizeof(float));

        var format = HdrFormat;
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSetings ? textureLoaderSetings : TextureLoaderSettings.Default;
        
        return new TextureData(id, image.Width, image.Height, pitch, format, settings.ShouldMipMap, new[] { bytes });
    }
}
