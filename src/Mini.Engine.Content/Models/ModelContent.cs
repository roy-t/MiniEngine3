using System;
using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Content.Models;

internal sealed record ModelData(ContentId Id, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, IMaterial[] Materials)
    : IContentData;

internal sealed class ModelContent : IModel, IContent
{
    private readonly IContentDataLoader<ModelData> Loader;
    private IModel model;

    public ModelContent(ContentId id, Device device, IContentDataLoader<ModelData> loader)
    {
        this.Id = id;
        this.Loader = loader;

        this.Reload(device);
    }

    public ContentId Id { get; }

    public VertexBuffer<ModelVertex> Vertices => this.model.Vertices;
    public IndexBuffer<int> Indices => this.model.Indices;
    public Primitive[] Primitives => this.model.Primitives;
    public IMaterial[] Materials => this.model.Materials;

    [MemberNotNull(nameof(model))]
    public void Reload(Device device)
    {
        this.model?.Dispose();

        var data = this.Loader.Load(device, this.Id);
        this.model = new Model(device, data.Vertices, data.Indices, data.Primitives, data.Materials, data.Id.ToString());
    }

    public void Dispose()
    {
        this.model.Dispose();
    }
}
