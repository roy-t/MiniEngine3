using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures;

internal sealed class SdrTextureProcessor : IContentProcessor<ITexture, TextureContent, TextureLoaderSettings>
{
    private static readonly Guid HeaderSdr = new("{7AED564E-32B4-4F20-B14A-2D209F0BABBD}");

    private readonly Device Device;

    public SdrTextureProcessor(Device device)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<ITexture>();
    }

    public int Version => 6;
    public IContentTypeCache<ITexture> Cache { get; }

    public void Generate(ContentId id, TextureLoaderSettings settings, ContentWriter contentWriter, TrackingVirtualFileSystem fileSystem)
    {
        if (this.HasSupportedSdrExtension(id.Path))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Image.FromMemory(bytes);
            SdrTextureWriter.Write(contentWriter, HeaderSdr, this.Version, settings, fileSystem.GetDependencies(), image);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public ITexture Load(ContentId id, ContentHeader header, ContentReader reader)
    {
        ContentProcessor.ValidateHeader(HeaderSdr, this.Version, header);
        return SdrTextureReader.Read(this.Device, id, TranscodeFormats.BC7_RGBA, reader);        
    }

    public TextureContent Wrap(ContentId id, ITexture content, TextureLoaderSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
    }

    public bool HasSupportedSdrExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => true,
            _ => false
        };
    }


}
