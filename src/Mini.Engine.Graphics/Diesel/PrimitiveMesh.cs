using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;

public sealed class PrimitiveMesh : IDisposable
{
    public readonly IndexBuffer<int> Indices;
    public readonly int IndexCount;

    public readonly VertexBuffer<PrimitiveVertex> Vertices;
    public readonly int VertexCount;
    public readonly BoundingBox Bounds;
    
    public PrimitiveMesh(Device device, ReadOnlyMemory<PrimitiveVertex> vertices, ReadOnlyMemory<int> indices, BoundingBox bounds, string name)
    {
        this.Indices = new IndexBuffer<int>(device, name);
        this.IndexCount = indices.Length;

        this.Vertices = new VertexBuffer<PrimitiveVertex>(device, name);
        this.VertexCount = vertices.Length;

        this.Bounds = bounds;

        this.Vertices.MapData(device.ImmediateContext, vertices.Span);
        this.Indices.MapData(device.ImmediateContext, indices.Span);
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();
    }
}