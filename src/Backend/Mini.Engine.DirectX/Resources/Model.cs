using System;

namespace Mini.Engine.DirectX.Resources;

public sealed record Primitive(string Name, int MaterialIndex, int IndexOffset, int IndexCount);

public sealed class Model : IModel
{
    public Model(Device device, ModelVertex[] vertices, int[] indices, Primitive[] primitives, IMaterial[] materials, string name)
    {
        this.Indices = new IndexBuffer<int>(device, $"indices_{name}");
        this.Vertices = new VertexBuffer<ModelVertex>(device, $"vertices_{name}");

        this.Primitives = primitives;
        this.Materials = materials;

        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public VertexBuffer<ModelVertex> Vertices { get; }
    public IndexBuffer<int> Indices { get; }
    public Primitive[] Primitives { get;  }
    public IMaterial[] Materials { get; }

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
