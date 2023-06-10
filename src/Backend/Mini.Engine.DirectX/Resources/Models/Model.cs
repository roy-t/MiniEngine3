using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Resources.Models;

public sealed record ModelPart(string Name, BoundingBox Bounds, int MaterialIndex, int IndexOffset, int IndexCount);

public sealed class Model : IModel
{
    public Model(Device device, BoundingBox bounds, ReadOnlyMemory<ModelVertex> vertices, ReadOnlyMemory<int> indices, IReadOnlyList<ModelPart> primitives, IReadOnlyList<ILifetime<IMaterial>> materials, string name)
    {
        this.Indices = new IndexBuffer<int>(device, name);
        this.Vertices = new VertexBuffer<ModelVertex>(device, name);
        this.Bounds = bounds;
        this.Primitives = primitives;
        this.Materials = materials;

        this.Vertices.MapData(device.ImmediateContext, vertices.Span);
        this.Indices.MapData(device.ImmediateContext, indices.Span);
    }

    public VertexBuffer<ModelVertex> Vertices { get; }
    public IndexBuffer<int> Indices { get; }
    public BoundingBox Bounds { get; set; } 
    public IReadOnlyList<ModelPart> Primitives { get; }
    public IReadOnlyList<ILifetime<IMaterial>> Materials { get; }

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
