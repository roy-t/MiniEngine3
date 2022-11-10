using Mini.Engine.Content.Materials;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX.Resources.Models;

namespace Mini.Engine.Content.v2.Materials;
internal sealed class WavefrontMaterialProcessor : IManagedContentProcessor<IMaterial, MaterialContent, MaterialLoaderSettings>
{
    private static readonly Guid HeaderMaterial = new("{0124D18A-D3E6-48C4-A733-BD3881171B76}");
    private readonly WavefrontMaterialParser Parser;
    private readonly ContentManager Content;

    public WavefrontMaterialProcessor(ContentManager content)
    {
        this.Parser = new WavefrontMaterialParser();
        this.Cache = new ContentTypeCache<IMaterial>();
        this.Content = content;
    }

    public int Version => 1;
    public IContentTypeCache<IMaterial> Cache { get; }

    public void Generate(ContentId id, MaterialLoaderSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem)
    {
        if (this.HasSupportedExtension(id.Path))
        {
            var material = this.Parser.Parse(id, fileSystem);

            writer.WriteHeader(HeaderMaterial, this.Version, fileSystem.GetDependencies());
            writer.Write(settings);
            writer.Writer.Write(material.Name);
            writer.Write(material.Albedo);
            writer.Write(material.Metalicness);
            writer.Write(material.Normal);
            writer.Write(material.Roughness);
            writer.Write(material.AmbientOcclusion);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public IMaterial Load(ContentId id, ContentHeader header, ContentReader reader)
    {
        ContentProcessorUtilities.ValidateHeader(HeaderMaterial, this.Version, header);

        var settings = reader.ReadMaterialSettings();
        var name = reader.Reader.ReadString();
        var albedo = this.Content.LoadTexture(reader.ReadContentId(), settings.AlbedoFormat);
        var metalicness = this.Content.LoadTexture(reader.ReadContentId(), settings.MetalicnessFormat);
        var normal = this.Content.LoadTexture(reader.ReadContentId(), settings.NormalFormat);
        var roughness = this.Content.LoadTexture(reader.ReadContentId(), settings.RoughnessFormat);
        var ambientOcclusion = this.Content.LoadTexture(reader.ReadContentId(), settings.AmbientOcclusionFormat);

        return new Material(name, albedo, metalicness, normal, roughness, ambientOcclusion);
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
