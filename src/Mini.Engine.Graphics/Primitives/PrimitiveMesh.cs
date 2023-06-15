using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;


using MeshPart = Mini.Engine.Content.Shaders.Generated.Primitive.MeshPart;

namespace Mini.Engine.Graphics.Primitives;

public sealed class PrimitiveMesh : IDisposable
{
    public readonly IndexBuffer<int> Indices;
    public readonly int IndexCount;

    public readonly VertexBuffer<PrimitiveVertex> Vertices;
    public readonly int VertexCount;

    public readonly StructuredBuffer<MeshPart> Parts;
    public readonly ShaderResourceView<MeshPart> PartsView;
    public readonly int PartCount;

    public readonly BoundingBox Bounds;

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

        this.PartsView = this.Parts.CreateShaderResourceView();

        this.Bounds = bounds;
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();
        this.Parts.Dispose();
        this.PartsView.Dispose();
    }
}