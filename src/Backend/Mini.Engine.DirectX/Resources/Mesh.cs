using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Resources;

public sealed class Mesh : IMesh
{
    public Mesh(Device device, BoundingBox bounds, ModelVertex[] vertices, int[] indices, string name)
    {
        this.Indices = new IndexBuffer<int>(device, $"{name}_IB");
        this.Vertices = new VertexBuffer<ModelVertex>(device, $"{name}_IB");
        this.Bounds = bounds;

        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public VertexBuffer<ModelVertex> Vertices { get; }
    public IndexBuffer<int> Indices { get; }
    public BoundingBox Bounds { get; }

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
