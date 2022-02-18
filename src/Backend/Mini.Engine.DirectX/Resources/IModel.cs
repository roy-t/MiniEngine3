using System;
using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Resources;

public interface IModel : IDisposable
{
    VertexBuffer<ModelVertex> Vertices { get; }
    IndexBuffer<int> Indices { get; }
    BoundingBox Bounds { get; }
    Primitive[] Primitives { get; }    
    IMaterial[] Materials { get; }
}