using Mini.Engine.DirectX.Buffers;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Resources.Models;

public interface IMesh : IDeviceResource
{
    VertexBuffer<ModelVertex> Vertices { get; }
    IndexBuffer<int> Indices { get; }
    BoundingBox Bounds { get; }
}