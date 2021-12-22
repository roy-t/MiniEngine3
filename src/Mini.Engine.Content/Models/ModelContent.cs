using Mini.Engine.DirectX;

namespace Mini.Engine.Content.Models;

internal sealed class ModelContent : Model, IContent
{
    private readonly IContentDataLoader<ModelData> Loader;

    public ModelContent(ContentId id, Device device, IContentDataLoader<ModelData> loader, ModelData data)
        : base(device, data.Vertices, data.Indices, data.Primitives, data.Materials, id.ToString())
    {
        this.Loader = loader;
        this.Id = id;
    }

    public ContentId Id { get; }

    public void Reload(Device device)
    {
        var data = this.Loader.Load(this.Id);

        this.Primitives = data.Primitives;
        this.Materials = data.Materials;

        this.MapData(device.ImmediateContext, data.Vertices, data.Indices);
    }
}
