using Mini.Engine.Content.Materials;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX.Resources.Models;

namespace Mini.Engine.Content.v2.Materials;
internal sealed class WavefrontMaterialProcessor : IContentProcessor<IMaterial, MaterialContent, MaterialLoaderSettings>
{
    private static readonly Guid HeaderMaterial = new("{0124D18A-D3E6-48C4-A733-BD3881171B76}");

    public WavefrontMaterialProcessor()
    {
        this.Cache = new ContentTypeCache<IMaterial>();
    }

    public int Version => 1;
    public IContentTypeCache<IMaterial> Cache { get; }

    public void Generate(ContentId id, MaterialLoaderSettings settings, ContentWriter contentWriter, TrackingVirtualFileSystem fileSystem)
    {
        if (this.HasSupportedExtension(id.Path))
        {
            // TODO: copy the WavefrontMaterialDataLoader and instead of doing 'Load' only parse the file
            // and store the contentids of the textures that should be loaded
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public IMaterial Load(ContentId id, ContentHeader header, ContentReader reader)
    {
        // TODO: read the original NAME, settings and textures needed
        // then use the content manager itself to load the required textures
    }

    public MaterialContent Wrap(ContentId id, IMaterial content, MaterialLoaderSettings settings, ISet<string> dependencies)
    {
        return new MaterialContent(id, content, settings, dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (MaterialContent)original, fileSystem, writerReader);
    }

    public bool HasSupportedExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".mtl" => true,
            _ => false
        };
    }
}
