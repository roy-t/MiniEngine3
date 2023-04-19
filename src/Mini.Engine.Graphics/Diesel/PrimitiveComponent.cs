using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;
public struct PrimitiveComponent
{
    public ILifetime<Primitive> Primitive;
    public Entity Entity;
    public LifeCycle LifeCycle;
}

public struct Primitive : IDisposable
{
    public IndexBuffer<int> Indices;
    public VertexBuffer<PrimitiveVertex> Vertices;
    public BoundingBox Bounds;

    public void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
        this.Bounds = BoundingBox.Empty;
    }
}
