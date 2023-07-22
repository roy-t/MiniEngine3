using Mini.Engine.Content.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.IO;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models;

public interface IWavefrontData : IDisposable
{
    public BoundingBox Bounds { get; }
    public ReadOnlyMemory<ModelVertex> Vertices { get; }
    public ReadOnlyMemory<int> Indices { get; }
    public ReadOnlyMemory<ModelPart> Primitives { get; }
    public ReadOnlyMemory<ContentId> Materials { get; }
}

internal sealed record class WavefrontData(BoundingBox Bounds, ReadOnlyMemory<ModelVertex> Vertices, ReadOnlyMemory<int> Indices, ReadOnlyMemory<ModelPart> Primitives, ReadOnlyMemory<ContentId> Materials)
    : IWavefrontData
{
    public void Dispose() { }
}

internal sealed class WavefrontDataContent : IWavefrontData, IContent<IWavefrontData, ModelSettings>
{
    internal WavefrontDataContent(ContentId id, IWavefrontData data, ModelSettings settings, ISet<string> dependencies)
    {
        this.Id = id;
        this.Settings = settings;
        this.Dependencies = dependencies;

        this.Bounds = data.Bounds;
        this.Vertices = data.Vertices;
        this.Indices = data.Indices;
        this.Primitives = data.Primitives;
        this.Materials = data.Materials;
    }

    public BoundingBox Bounds { get; private set; }
    public ReadOnlyMemory<ModelVertex> Vertices { get; private set; }
    public ReadOnlyMemory<int> Indices { get; private set; }
    public ReadOnlyMemory<ModelPart> Primitives { get; private set; }
    public ReadOnlyMemory<ContentId> Materials { get; private set; }

    public ModelSettings Settings { get; }
    public ContentId Id { get; }
    public ISet<string> Dependencies { get; }

    public void Dispose()
    {
    }

    public void Reload(IWavefrontData content)
    {
        this.Bounds = content.Bounds;
        this.Vertices = content.Vertices;
        this.Indices = content.Indices;
        this.Primitives = content.Primitives;
        this.Materials = content.Materials;
    }
}

// TODO: this is basically a slimmed down version of WaveFrontModelProcessor for when
// you are only interested in the data in a wavefont file, and don't want to convert it to a GPU resource immediately
// but we should be able to share some code with WaveFrontModelProcessor.

internal sealed class WavefrontDataProcessor : ContentProcessor<IWavefrontData, WavefrontDataContent, ModelSettings>
{
    private const int ProcessorVersion = 3;
    private static readonly Guid ProcessorType = new("{3F9E5C56-4A00-4DD6-9703-40FD3DD7CE6E}");
    private readonly WavefrontModelParser Parser;

    public WavefrontDataProcessor(Device device, WavefrontModelParser parser)
        : base(device.Resources, ProcessorVersion, ProcessorType, ".obj")
    {
        this.Parser = parser;
    }

    public override WavefrontDataContent Wrap(ContentId id, IWavefrontData content, ModelSettings settings, ISet<string> dependencies)
    {
        return new WavefrontDataContent(id, content, settings, dependencies);
    }

    protected override IWavefrontData ReadBody(ContentId id, ModelSettings settings, ContentReader reader)
    {
        var bounds = reader.ReadBoundingBox();
        var vertices = reader.ReadModelVertices();
        var indices = reader.ReadIndices();
        var primitives = reader.ReadPrimitives();
        var materialIds = reader.ReadContentIds();

        return new WavefrontData(bounds, vertices, indices, primitives, materialIds);
    }

    protected override ModelSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadModelSettings();
    }

    protected override void WriteBody(ContentId id, ModelSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var model = this.Parser.Parse(id, fileSystem);

        writer.Write(model.Bounds);
        writer.Write(model.Vertices);
        writer.Write(model.Indices);
        writer.Write(model.Primitives);
        writer.Write(model.Materials);
    }

    protected override void WriteSettings(ContentId id, ModelSettings settings, ContentWriter writer)
    {
        writer.Write(settings);
    }
}
