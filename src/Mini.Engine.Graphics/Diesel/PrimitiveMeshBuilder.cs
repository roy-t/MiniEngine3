using System.Diagnostics;
using System.Numerics;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;

public sealed class PrimitiveMeshBuilder
{
    private readonly PrimitiveVertex[] VertexArray;
    private readonly int[] IndexArray;
    private readonly MeshPart[] PartArray;

    private BoundingBox bounds;

    private int vertexCount;
    private int indexCount;
    private int partCount;

    public PrimitiveMeshBuilder(int vertexCapacity = 1000, int indexCapacity = 2000, int partCapacity = 10)
    {
        this.VertexArray = new PrimitiveVertex[vertexCapacity];
        this.IndexArray = new int[indexCapacity];
        this.PartArray = new MeshPart[partCapacity];

        this.vertexCount = 0;
        this.indexCount = 0;
        this.partCount = 0;

        this.bounds = BoundingBox.Empty;
    }

    public void Add(ReadOnlyMemory<PrimitiveVertex> vertices, ReadOnlyMemory<int> indices)
    {
        this.EnsureCapacity(vertices.Length, indices.Length, 1);

        Add(vertices, this.VertexArray, ref this.vertexCount);
        Add(indices, this.IndexArray, ref this.indexCount);

        this.PartArray[this.partCount] = new MeshPart(this.partCount, indices.Length);
        this.partCount++;

        var vertexSpan = vertices.Span;
        for(var i = 0; i < vertexSpan.Length; i++)
        {
            var min = Vector3.Min(this.bounds.Min, vertexSpan[i].Position);
            var max = Vector3.Min(this.bounds.Max, vertexSpan[i].Position);            
            this.bounds = new BoundingBox(min, max);
        }
    }

    public PrimitiveMesh Build(Device device, string name)
    {
        Debug.Assert(this.vertexCount > 0 && this.indexCount > 0 && this.partCount > 0 && this.bounds != BoundingBox.Empty);

        var vertices = new ReadOnlyMemory<PrimitiveVertex>(this.VertexArray, 0, this.vertexCount);
        var indices = new ReadOnlyMemory<int>(this.IndexArray, 0, this.indexCount);
        var parts = new ReadOnlyMemory<MeshPart>(this.PartArray, 0, this.partCount);

        return new PrimitiveMesh(device, vertices, indices, parts, this.bounds, name);
    }

    private void EnsureCapacity(int vertexIncrease, int indexIncrease, int partIncrease)
    {
        EnsureCapacity(this.vertexCount, vertexIncrease, this.VertexArray);
        EnsureCapacity(this.indexCount, indexIncrease, this.IndexArray);
        EnsureCapacity(this.partCount, partIncrease, this.PartArray);
    }

    private static void EnsureCapacity<T>(int count, int increase, T[] array)
    {
        if (count + increase < array.Length)
        {
            return;
        }

        var targetSize = Math.Max(array.Length * 2, (count + increase) * 2);

        Array.Resize<T>(ref array, targetSize);
    }

    private static void Add<T>(ReadOnlyMemory<T> source, T[] destination, ref int currentCount)
    {
        var memory = new Span<T>(destination, currentCount, destination.Length);
        source.Span.CopyTo(memory);

        currentCount += source.Length;
    }
}
