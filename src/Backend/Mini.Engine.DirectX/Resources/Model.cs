using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.DirectX.Buffers;

namespace Mini.Engine.DirectX.Resources;

public sealed class Primitive
{
    public Primitive(string name, BoundingBox bounds, int materialIndex, int indexOffset, int indexCount)
    {
        this.Name = name;
        this.Bounds = bounds;
        this.MaterialIndex = materialIndex;
        this.IndexOffset = indexOffset;
        this.IndexCount = indexCount;
    }

    public string Name { get; }
    public BoundingBox Bounds { get; }
    public int MaterialIndex { get; }
    public int IndexOffset { get; }
    public int IndexCount { get; }
}

public sealed class Model : IModel
{
    public Model(Device device, ModelVertex[] vertices, int[] indices, Primitive[] primitives, IMaterial[] materials, string name)
    {
        this.Indices = new IndexBuffer<int>(device, $"{name}_IB");
        this.Vertices = new VertexBuffer<ModelVertex>(device, $"{name}_IB");

        this.Primitives = primitives;
        this.Materials = materials;

        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public VertexBuffer<ModelVertex> Vertices { get; }
    public IndexBuffer<int> Indices { get; }
    public Primitive[] Primitives { get; }
    public IMaterial[] Materials { get; }

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
