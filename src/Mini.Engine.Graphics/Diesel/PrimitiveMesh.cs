using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;


using MeshPart = Mini.Engine.Content.Shaders.Generated.Primitive.MeshPart;

namespace Mini.Engine.Graphics.Diesel;

public sealed class PrimitiveMesh : IDisposable
{
    public readonly IndexBuffer<int> Indices;
    public readonly int IndexCount;

    public readonly VertexBuffer<PrimitiveVertex> Vertices;
    public readonly int VertexCount;

    public readonly StructuredBuffer<MeshPart> Parts;
    public readonly int PartCount;

    public readonly BoundingBox Bounds;

    public PrimitiveMesh(Device device, ReadOnlySpan<PrimitiveVertex> vertices, ReadOnlySpan<int> indices, Color4 color, BoundingBox bounds, string name)
        : this(device, vertices, indices, new MeshPart[] { new MeshPart { Offset = 0, Length = (uint)indices.Length, Color = color } }, bounds, name) { }


    public PrimitiveMesh(Device device, ReadOnlySpan<PrimitiveVertex> vertices, ReadOnlySpan<int> indices, ReadOnlySpan<MeshPart> parts, BoundingBox bounds, string name)
    {
        this.Indices = new IndexBuffer<int>(device, name);
        this.Indices.MapData(device.ImmediateContext, indices);
        this.IndexCount = indices.Length;

        this.Vertices = new VertexBuffer<PrimitiveVertex>(device, name);
        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.VertexCount = vertices.Length;

        this.Parts = new StructuredBuffer<MeshPart>(device, name);
        this.Parts.MapData(device.ImmediateContext, parts);
        this.PartCount = parts.Length;

        this.Bounds = bounds;
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();
        this.Parts.Dispose();
    }
}