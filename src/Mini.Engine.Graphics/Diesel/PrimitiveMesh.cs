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
    public readonly MeshPart[] Parts;

    public PrimitiveMesh(Device device, ReadOnlyMemory<PrimitiveVertex> vertices, ReadOnlyMemory<int> indices, BoundingBox bounds, string name)
        : this(device, vertices, indices, new MeshPart[] { new MeshPart(0, indices.Length) }, bounds, name) { }


    public PrimitiveMesh(Device device, ReadOnlyMemory<PrimitiveVertex> vertices, ReadOnlyMemory<int> indices, ReadOnlyMemory<MeshPart> parts, BoundingBox bounds, string name)
    {
        this.Indices = new IndexBuffer<int>(device, name);
        this.IndexCount = indices.Length;

        this.Vertices = new VertexBuffer<PrimitiveVertex>(device, name);
        this.VertexCount = vertices.Length;

        this.Vertices.MapData(device.ImmediateContext, vertices.Span);
        this.Indices.MapData(device.ImmediateContext, indices.Span);

        this.Parts = parts.ToArray();
        this.Bounds = bounds;
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();
    }
}