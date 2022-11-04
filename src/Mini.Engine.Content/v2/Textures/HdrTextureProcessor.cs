using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using ColorComponents = StbImageSharp.ColorComponents;
using ImageResultFloat = StbImageSharp.ImageResultFloat;

namespace Mini.Engine.Content.v2.Textures;
internal sealed class HdrTextureProcessor : IContentProcessor<TextureContent, TextureLoaderSettings>
{
    private static readonly Guid HeaderHdr = new("{650531A2-0DF5-4E36-85AB-F5525464F962}");

    private readonly Device Device;

    public HdrTextureProcessor(Device device)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<TextureContent>();
    }

    public int Version => 1;
    public IContentTypeCache<TextureContent> Cache { get; }

    public void Generate(ContentId id, TextureLoaderSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem)
    {
        if (this.HasSupportedHdrExtension(id.Path))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = ImageResultFloat.FromMemory(bytes, ColorComponents.RedGreenBlue);
            HdrTextureWriter.Write(writer, HeaderHdr, this.Version, settings, fileSystem.GetDependencies(), image);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public TextureContent Load(ContentId contentId, ContentHeader header, ContentReader reader)
    {
        if (header.Type == HeaderHdr)
        {

            var (settings, texture) = HdrTextureReader.Read(this.Device, contentId, reader);
            return new TextureContent(contentId, texture, settings, header.Dependencies);
        }

        throw new NotSupportedException($"Unexpected header: {header}");
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        TextureReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
    }


    public bool HasSupportedHdrExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".hdr" => true,
            _ => false
        };
    }
}
