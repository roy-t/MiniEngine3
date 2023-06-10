using System.Diagnostics;
using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Primitives;

using MeshPart = Mini.Engine.Content.Shaders.Generated.Primitive.MeshPart;

public interface IPrimitiveMeshPartBuilder
{
    void AddIndex(int index);
    int AddVertex(PrimitiveVertex vertex);
    int AddVertex(Vector3 position, Vector3 normal);
    void Complete(Color4 color);
}

public sealed class PrimitiveMeshBuilder
{
    private readonly ArrayBuilder<PrimitiveVertex> Vertices;
    private readonly ArrayBuilder<int> Indices;
    private readonly ArrayBuilder<MeshPart> Parts;
    private BoundingBox bounds;

    public PrimitiveMeshBuilder(int vertexCapacity = 24, int indexCapacity = 36, int partCapacity = 1)
    {
        this.Vertices = new ArrayBuilder<PrimitiveVertex>(vertexCapacity);
        this.Indices = new ArrayBuilder<int>(indexCapacity);
        this.Parts = new ArrayBuilder<MeshPart>(partCapacity);

        this.bounds = BoundingBox.Empty;
    }

    public void Add(ReadOnlySpan<PrimitiveVertex> vertices, ReadOnlySpan<int> indices, Color4 color)
    {
        var vertexOffset = this.Vertices.Length;

        for (var i = 0; i < vertices.Length; i++)
        {
            this.AddVertex(vertices[i]);
        }

        for (var i = 0; i < indices.Length; i++)
        {
            this.AddIndex(indices[i], vertexOffset);
        }

        this.AddPart(vertexOffset, vertices.Length, color);
    }

    private void AddVertex(PrimitiveVertex vertex)
    {
        this.Vertices.Add(vertex);
        var min = Vector3.Min(this.bounds.Min, vertex.Position);
        var max = Vector3.Max(this.bounds.Max, vertex.Position);
        this.bounds = new BoundingBox(min, max);
    }

    private void AddIndex(int index, int vertexOffset)
    {
        this.Indices.Add(index + vertexOffset);
    }

    private void AddPart(int vertexOffset, int vertexCount, Color4 color)
    {
        this.Parts.Add(new MeshPart
        {
            Offset = (uint)vertexOffset,
            Length = (uint)vertexCount,
            Color = color
        });
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

    public PrimitiveMeshPartBuilder StartPart()
    {
        return new PrimitiveMeshPartBuilder(this, this.Vertices.Length);
    }

    public sealed class PrimitiveMeshPartBuilder : IPrimitiveMeshPartBuilder
    {
        private readonly PrimitiveMeshBuilder Parent;
        private readonly int VertexOffset;
        private int vertexLength;
        private int indexLength;

        public PrimitiveMeshPartBuilder(PrimitiveMeshBuilder parent, int vertexOffset)
        {
            this.Parent = parent;
            this.VertexOffset = vertexOffset;
            this.vertexLength = 0;
            this.indexLength = 0;
        }

        public int AddVertex(Vector3 position, Vector3 normal)
        {
            return this.AddVertex(new PrimitiveVertex(position, normal));
        }

        public int AddVertex(PrimitiveVertex vertex)
        {
            this.Parent.AddVertex(vertex);
            return this.vertexLength++;
        }

        public void AddIndex(int index)
        {
            this.Parent.AddIndex(index, this.VertexOffset);
            this.indexLength++;
        }

        public void Complete(Color4 color)
        {
            this.Parent.AddPart(this.VertexOffset, this.vertexLength, color);
        }

        public void Layout(params Transform[] transforms)
        {
            Debug.Assert(transforms.Length > 0);

            var startVertex = this.Parent.Vertices.Length - this.vertexLength;
            var vLength = this.vertexLength;
            var startIndex = this.Parent.Indices.Length - this.indexLength;
            var iLength = this.indexLength;

            for (var t = 1; t < transforms.Length; t++)
            {
                var matrix = transforms[t].GetMatrix();
                var repeatOffset = t * vLength;

                for (var i = 0; i < iLength; i++)
                {
                    this.AddIndex((this.Parent.Indices[startIndex + i] - this.VertexOffset) + repeatOffset);
                }
                
                for (var v = 0; v < vLength; v++)
                {
                    var original = this.Parent.Vertices[startVertex + v];
                    var position = Vector3.Transform(original.Position, matrix);
                    var normal = Vector3.TransformNormal(original.Normal, matrix);

                    this.AddVertex(new PrimitiveVertex(position, normal));
                }
            }

            var firstMatrix = transforms[0].GetMatrix();
            for (var v = 0; v < vLength; v++)
            {
                var original = this.Parent.Vertices[startVertex + v];
                var position = Vector3.Transform(original.Position, firstMatrix);
                var normal = Vector3.TransformNormal(original.Normal, firstMatrix);

                this.Parent.Vertices[startVertex + v] = new PrimitiveVertex(position, normal);                
            }
        }
    }
}
