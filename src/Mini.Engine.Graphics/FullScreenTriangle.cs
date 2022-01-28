using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PostProcessVertex
{
    public Vector3 Position;
    public Vector2 TexCoord;

    public PostProcessVertex(Vector3 position, Vector2 texCoord)
    {
        this.Position = position;
        this.TexCoord = texCoord;
    }

    public static readonly InputElementDescription[] Elements = new InputElementDescription[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float,  sizeof(float) * 0, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, sizeof(float) * 3, 0, InputClassification.PerVertexData, 0)
    };
}


[Service]
public sealed class FullScreenTriangle : IDisposable
{
    public readonly VertexBuffer<PostProcessVertex> Vertices;
    public readonly IndexBuffer<short> Indices;

    public FullScreenTriangle(Device device)
    {
        this.Vertices = new VertexBuffer<PostProcessVertex>(device, $"{nameof(FullScreenTriangle)}_VB");
        this.Indices = new IndexBuffer<short>(device, $"{nameof(FullScreenTriangle)}_IB");

        var vertices = new PostProcessVertex[]
        {
                new PostProcessVertex(new Vector3(3, -1, 0), new Vector2(2, 1)),
                new PostProcessVertex(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new PostProcessVertex(new Vector3(-1, 3, 0), new Vector2(0, -1)),
        };

        var indices = new short[] { 0, 1, 2 };

        this.Vertices.MapData(device.ImmediateContext, vertices);
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public static int PrimitiveCount => 3;
    public static int PrimitiveOffset => 0;
    public static int VertexOffset => 0;

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();
    }
}
