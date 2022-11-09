using Mini.Engine.Content.Models;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;

namespace Mini.Engine.Content.v2.Models;
internal sealed class WaveFrontModelProcessor : IUnmanagedContentProcessor<IModel, ModelContent, ModelLoaderSettings>
{
    private static readonly Guid HeaderModel = new("{A855A352-8403-4B09-A87B-648F4901962E}");
    private readonly WavefrontModelParser Parser;
    private readonly Device Device;
    private readonly ContentManager Content;

    public WaveFrontModelProcessor(Device device, ContentManager content)
    {
        this.Device = device;
        this.Content = content;

        this.Parser = new WavefrontModelParser();
        this.Cache = new ContentTypeCache<ILifetime<IModel>>();
    }

    public int Version => 1;
    public IContentTypeCache<ILifetime<IModel>> Cache { get; }

    public void Generate(ContentId id, ModelLoaderSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem)
    {
        if (this.HasSupportedExtension(id.Path))
        {
            var model = this.Parser.Parse(id, fileSystem);

            writer.WriteHeader(HeaderModel, this.Version, fileSystem.GetDependencies());
            writer.Write(settings);

            writer.Write(model.Bounds);
            writer.Write(model.Vertices);
            writer.Write(model.Indices);
            writer.Write(model.Primitives);
            writer.Write(model.Materials);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public IModel Load(ContentId id, ContentHeader header, ContentReader reader)
    {
        ContentProcessor.ValidateHeader(HeaderModel, this.Version, header);

        var settings = reader.ReadModelSettings();
        var bounds = reader.ReadBoundingBox();
        var vertices = reader.ReadModelVertices();
        var indices = reader.ReadIndices();
        var primitives = reader.ReadPrimitives();
        var materialIds = reader.ReadContentIds();

        var materials = new IMaterial[materialIds.Count];
        for (var i = 0; i < materials.Length; i++)
        {
            materials[i] = this.Content.LoadMaterial(materialIds[i], settings.MaterialSettings);
        }

        return new Model(this.Device, bounds, vertices, indices, primitives, materials, id.ToString());
    }

    public ModelContent Wrap(ContentId id, IModel content, ModelLoaderSettings settings, ISet<string> dependencies)
    {
        return new ModelContent(id, content, settings, dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (ModelContent)original, fileSystem, writerReader);
    }

    public bool HasSupportedExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".obj" => true,
            _ => false
        };
    }
}
