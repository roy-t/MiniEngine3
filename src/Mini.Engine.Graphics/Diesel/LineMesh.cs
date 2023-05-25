using System.Diagnostics;
using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.Diesel;

public sealed class LineMesh : IDisposable
{
    public readonly VertexBuffer<Vector3> Vertices;
    public readonly int VertexCount;

    public LineMesh(Device device, string name, params Vector3[] vertices)
    {
        Debug.Assert(vertices.Length > 1);

        this.Vertices = new VertexBuffer<Vector3>(device, name);
        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.VertexCount = vertices.Length;
    }

    public static readonly InputElementDescription[] Elements = new InputElementDescription[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
    };

    public void Dispose()
    {
        this.Vertices.Dispose();
    }
}