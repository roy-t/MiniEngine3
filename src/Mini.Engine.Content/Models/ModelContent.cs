using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models;

internal sealed record ModelData(ContentId Id, BoundingBox Bounds, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, IMaterial[] Materials)
    : IContentData;

internal sealed class ModelContent : IModel, IContent
{
    private readonly IContentDataLoader<ModelData> Loader;
    private readonly ILoaderSettings Settings;
    private IModel model;

    public ModelContent(ContentId id, Device device, IContentDataLoader<ModelData> loader, ILoaderSettings settings)
    {
        this.Id = id;
        this.Loader = loader;
        this.Settings = settings;
        this.Reload(device);
    }

    public ContentId Id { get; }

    public VertexBuffer<ModelVertex> Vertices => this.model.Vertices;
    public IndexBuffer<int> Indices => this.model.Indices;
    public BoundingBox Bounds => this.model.Bounds;
    public Primitive[] Primitives => this.model.Primitives;
    public IMaterial[] Materials => this.model.Materials;    

    [MemberNotNull(nameof(model))]
    public void Reload(Device device)
    {
        this.model?.Dispose();

        var data = this.Loader.Load(device, this.Id, this.Settings);
        this.model = new Model(device, data.Bounds, data.Vertices, data.Indices, data.Primitives, data.Materials, data.Id.ToString());
    }

    public void Dispose()
    {
        this.model.Dispose();
    }

    public override string ToString()
    {
        return $"Model: {this.Id}";
    }
}
