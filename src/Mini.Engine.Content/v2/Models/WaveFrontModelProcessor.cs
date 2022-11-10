using Mini.Engine.Content.Models;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2.Models;
internal sealed class WaveFrontModelProcessor : UnmanagedContentProcessor<IModel, ModelContent, ModelLoaderSettings>
{
    private const int ProcessorVersion = 1;
    private static readonly Guid ProcessorType = new("{A855A352-8403-4B09-A87B-648F4901962E}");
    private readonly WavefrontModelParser Parser;
    private readonly Device Device;
    private readonly ContentManager Content;

    public WaveFrontModelProcessor(Device device, ContentManager content)
        : base(ProcessorVersion, ProcessorType, ".obj")
    {
        this.Device = device;
        this.Content = content;
        this.Parser = new WavefrontModelParser();        
    }

    protected override void WriteSettings(ContentId id, ModelLoaderSettings settings, ContentWriter writer)
    {
        writer.Write(settings);
    }

    protected override void WriteBody(ContentId id, ModelLoaderSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var model = this.Parser.Parse(id, fileSystem);

        writer.Write(model.Bounds);
        writer.Write(model.Vertices);
        writer.Write(model.Indices);
        writer.Write(model.Primitives);
        writer.Write(model.Materials);
    }

    protected override ModelLoaderSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadModelSettings();
    }

    protected override IModel ReadBody(ContentId id, ModelLoaderSettings settings, ContentReader reader)
    {
        var bounds = reader.ReadBoundingBox();
        var vertices = reader.ReadModelVertices();
        var indices = reader.ReadIndices();
        var primitives = reader.ReadPrimitives();
        var materialIds = reader.ReadContentIds();

        var materials = new IMaterial[materialIds.Length];
        for (var i = 0; i < materials.Length; i++)
        {
            materials[i] = this.Content.LoadMaterial(materialIds[i], settings.MaterialSettings);
        }

        return new Model(this.Device, bounds, vertices, indices, primitives, materials, id.ToString());
    }  

    public override ModelContent Wrap(ContentId id, IModel content, ModelLoaderSettings settings, ISet<string> dependencies)
    {
        return new ModelContent(id, content, settings, dependencies);
    }    
}
