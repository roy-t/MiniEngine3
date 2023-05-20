using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;

using MeshPart = Mini.Engine.Content.Shaders.Generated.Primitive.MeshPart;

public sealed class PrimitiveMeshBuilder
{
    private readonly ArrayBuilder<PrimitiveVertex> Vertices;
    private readonly ArrayBuilder<int> Indices;
    private readonly ArrayBuilder<MeshPart> Parts;
    private BoundingBox bounds;

    public PrimitiveMeshBuilder(int vertexCapacity = 1000, int indexCapacity = 2000, int partCapacity = 10)
    {
        this.Vertices = new ArrayBuilder<PrimitiveVertex>(vertexCapacity);
        this.Indices = new ArrayBuilder<int>(indexCapacity);
        this.Parts = new ArrayBuilder<MeshPart>(partCapacity);

        this.bounds = BoundingBox.Empty;
    }

    public void Add(ReadOnlySpan<PrimitiveVertex> vertices, ReadOnlySpan<int> indices, Color4 color)
    {
        this.Parts.Add(new MeshPart
        {
            Offset = (uint)this.Vertices.Length,
            Length = (uint)vertices.Length,
            Color = color
        });

        this.Vertices.Add(vertices);
        this.Indices.Add(indices);

        for (var i = 0; i < vertices.Length; i++)
        {
            var min = Vector3.Min(this.bounds.Min, vertices[i].Position);
            var max = Vector3.Min(this.bounds.Max, vertices[i].Position);
            this.bounds = new BoundingBox(min, max);
        }
    }

    public PrimitiveMesh Build(Device device, string name)
    {
        Debug.Assert(this.Vertices.Length > 0 && this.Indices.Length > 0 && this.Parts.Length > 0 && this.bounds != BoundingBox.Empty);

        var vertices = this.Vertices.Build();
        var indices = this.Indices.Build();
        var parts = this.Parts.Build();

        return new PrimitiveMesh(device, vertices, indices, parts, this.bounds, name);
    }
}
