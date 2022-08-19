using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Resources.Models;

public sealed class Mesh : IMesh
{
    public Mesh(Device device, BoundingBox bounds, ModelVertex[] vertices, int[] indices, string user, string name)
    {
        this.Indices = new IndexBuffer<int>(device, $"{user}::{name}");
        this.Vertices = new VertexBuffer<ModelVertex>(device, $"{user}::{name}");
        this.Bounds = bounds;

        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public VertexBuffer<ModelVertex> Vertices { get; }
    public IndexBuffer<int> Indices { get; }
    public BoundingBox Bounds { get; set; }

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
