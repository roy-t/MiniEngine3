using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using ColorComponents = StbImageSharp.ColorComponents;
using ImageResultFloat = StbImageSharp.ImageResultFloat;

namespace Mini.Engine.Content.v2.Textures;
internal sealed class HdrTextureProcessor : IUnmanagedContentProcessor<ITexture, TextureContent, TextureLoaderSettings>
{
    private static readonly Guid HeaderHdr = new("{650531A2-0DF5-4E36-85AB-F5525464F962}");

    private readonly Device Device;

    public HdrTextureProcessor(Device device)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<ILifetime<ITexture>>();
    }

    public int Version => 1;
    public IContentTypeCache<ILifetime<ITexture>> Cache { get; }

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

    public ITexture Load(ContentId contentId, ContentHeader header, ContentReader reader)
    {
        ContentProcessor.ValidateHeader(HeaderHdr, this.Version, header);
        return HdrTextureReader.Read(this.Device, contentId, reader);
    }

    public TextureContent Wrap(ContentId id, ITexture content, TextureLoaderSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
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
