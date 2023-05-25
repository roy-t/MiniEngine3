using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;
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
        var vertexOffset = this.Vertices.Length;
        
        this.Vertices.Add(vertices);

        for (var i = 0; i < indices.Length; i++)
        {
            this.Indices.Add(indices[i] + vertexOffset);
        }

        this.Parts.Add(new MeshPart
        {
            Offset = (uint)vertexOffset,
            Length = (uint)vertices.Length,
            Color = color
        });


        for (var i = 0; i < vertices.Length; i++)
        {
            var min = Vector3.Min(this.bounds.Min, vertices[i].Position);
            var max = Vector3.Max(this.bounds.Max, vertices[i].Position);
            this.bounds = new BoundingBox(min, max);
        }
    }

    public ILifetime<PrimitiveMesh> Build(Device device, string name, out BoundingBox bounds)
    {
        Debug.Assert(this.Vertices.Length > 0 && this.Indices.Length > 0 && this.Parts.Length > 0 && this.bounds != BoundingBox.Empty);

        var vertices = this.Vertices.Build();
        var indices = this.Indices.Build();
        var parts = this.Parts.Build();

        var mesh = new PrimitiveMesh(device, vertices, indices, parts, this.bounds, name);
        bounds = this.bounds;
        return device.Resources.Add(mesh);
    }
}
