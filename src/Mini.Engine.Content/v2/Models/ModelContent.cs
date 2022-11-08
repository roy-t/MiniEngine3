using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Content.Models;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Models;
using Vortice.Mathematics;

namespace Mini.Engine.Content.v2.Models;
public sealed class ModelContent : IModel, IContent<IModel, ModelLoaderSettings>
{
    private IModel original;

    public ModelContent(ContentId id, IModel original, ModelLoaderSettings settings, ISet<string> dependencies)
    {
        this.Id = id;
        this.Settings = settings;
        this.Dependencies = dependencies;

        this.Reload(original);
    }

    [MemberNotNull(nameof(original))]
    public void Reload(IModel original)
    {
        this.Dispose();
        this.original = original;
    }

    public ContentId Id { get; }
    public ModelLoaderSettings Settings { get; }    
    public ISet<string> Dependencies { get; }

    public IReadOnlyList<Primitive> Primitives => this.original.Primitives;
    public IReadOnlyList<IMaterial> Materials => this.original.Materials;
    public VertexBuffer<ModelVertex> Vertices => this.original.Vertices;
    public IndexBuffer<int> Indices => this.original.Indices;
    public BoundingBox Bounds => this.original.Bounds;

    public void Dispose()
    {
        this.original.Dispose();
    }    
}
