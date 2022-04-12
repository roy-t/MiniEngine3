using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Resources;

public sealed record Primitive(string Name, BoundingBox Bounds, int MaterialIndex, int IndexOffset, int IndexCount);

public sealed class Model : IModel
{
    public Model(Device device, BoundingBox bounds, ModelVertex[] vertices, int[] indices, Primitive[] primitives, IMaterial[] materials, string name)
    {
        this.Indices = new IndexBuffer<int>(device, name);
        this.Vertices = new VertexBuffer<ModelVertex>(device, name);
        this.Bounds = bounds;
        this.Primitives = primitives;
        this.Materials = materials;

        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public VertexBuffer<ModelVertex> Vertices { get; }
    public IndexBuffer<int> Indices { get; }
    public BoundingBox Bounds { get; }
    public Primitive[] Primitives { get; }
    public IMaterial[] Materials { get; }

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
