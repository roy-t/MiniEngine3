using System.Diagnostics;
using System.Numerics;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;

public sealed class PrimitiveMeshBuilder
{
    private PrimitiveVertex[] vertexArray;
    private int[] indexArray;
    private MeshPart[] partArray;

    private BoundingBox bounds;

    private int vertexCount;
    private int indexCount;
    private int partCount;

    public PrimitiveMeshBuilder(int vertexCapacity = 1000, int indexCapacity = 2000, int partCapacity = 10)
    {
        this.vertexArray = new PrimitiveVertex[vertexCapacity];
        this.indexArray = new int[indexCapacity];
        this.partArray = new MeshPart[partCapacity];

        this.vertexCount = 0;
        this.indexCount = 0;
        this.partCount = 0;

        this.bounds = BoundingBox.Empty;
    }

    public void Add(ReadOnlySpan<PrimitiveVertex> vertices, ReadOnlySpan<int> indices)
    {
        this.EnsureCapacity(vertices.Length, indices.Length, 1);

        Add(vertices, this.vertexArray, ref this.vertexCount);
        Add(indices, this.indexArray, ref this.indexCount);

        this.partArray[this.partCount] = new MeshPart(this.partCount, indices.Length);
        this.partCount++;

        for(var i = 0; i < vertices.Length; i++)
        {
            var min = Vector3.Min(this.bounds.Min, vertices[i].Position);
            var max = Vector3.Min(this.bounds.Max, vertices[i].Position);            
            this.bounds = new BoundingBox(min, max);
        }
    }

    public PrimitiveMesh Build(Device device, string name)
    {
        Debug.Assert(this.vertexCount > 0 && this.indexCount > 0 && this.partCount > 0 && this.bounds != BoundingBox.Empty);

        var vertices = new ReadOnlyMemory<PrimitiveVertex>(this.vertexArray, 0, this.vertexCount);
        var indices = new ReadOnlyMemory<int>(this.indexArray, 0, this.indexCount);
        var parts = new ReadOnlyMemory<MeshPart>(this.partArray, 0, this.partCount);

        return new PrimitiveMesh(device, vertices, indices, parts, this.bounds, name);
    }

    private void EnsureCapacity(int vertexIncrease, int indexIncrease, int partIncrease)
    {
        EnsureCapacity(this.vertexCount, vertexIncrease, ref this.vertexArray);
        EnsureCapacity(this.indexCount, indexIncrease, ref this.indexArray);
        EnsureCapacity(this.partCount, partIncrease, ref this.partArray);
    }

    private static void EnsureCapacity<T>(int count, int increase, ref T[] array)
    {
        if (count + increase < array.Length)
        {
            return;
        }

        var targetSize = Math.Max(array.Length * 2, (count + increase) * 2);

        Array.Resize<T>(ref array, targetSize);
    }

    private static void Add<T>(ReadOnlySpan<T> source, T[] destination, ref int currentCount)
    {
        var memory = new Span<T>(destination, currentCount, destination.Length);
        source.CopyTo(memory);

        currentCount += source.Length;
    }
}
