using System;

namespace Mini.Engine.DirectX.Resources;

public sealed record Primitive(string Name, int MaterialIndex, int IndexOffset, int IndexCount);

public class Model : IDisposable
{
    public Model(Device device, ModelVertex[] vertices, int[] indices, Primitive[] primitives, IMaterial[] materials, string name)
    {
        this.Indices = new IndexBuffer<int>(device, $"indices_{name}");
        this.Vertices = new VertexBuffer<ModelVertex>(device, $"vertices_{name}");

        this.Primitives = primitives;
        this.Materials = materials;

        this.MapData(device.ImmediateContext, vertices, indices);
    }

    public VertexBuffer<ModelVertex> Vertices { get; protected set; }
    public IndexBuffer<int> Indices { get; protected set; }
    public Primitive[] Primitives { get; protected set; }
    public IMaterial[] Materials { get; protected set; }

    public int PrimitiveCount => this.Primitives.Length;

    protected void MapData(DeviceContext context, ModelVertex[] vertices, int[] indices)
    {
        this.Vertices.MapData(context, vertices);
        this.Indices.MapData(context, indices);
    }

    public virtual void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();

        GC.SuppressFinalize(this);
    }
}
