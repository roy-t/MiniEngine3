using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ModelVertex
{
    public Vector3 Position;
    public Vector2 Texcoord;
    public Vector3 Normal;

    public ModelVertex(Vector3 position, Vector2 texcoord, Vector3 normal)
    {
        this.Position = position;
        this.Texcoord = texcoord;
        this.Normal = normal;
    }

    public static readonly InputElementDescription[] Elements = new InputElementDescription[]
    {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 3 * sizeof(float), 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 5 * sizeof(float), 0, InputClassification.PerVertexData, 0)
    };
}

public sealed record Material(string Name, Texture2D Albedo, Texture2D Metalicness, Texture2D Normal, Texture2D Roughness, Texture2D AmbientOcclusion);
public sealed record Primitive(string Name, int MaterialIndex, int IndexOffset, int IndexCount);

public abstract class Model
{
    protected Model(Device device)
    {
        this.Indices = new IndexBuffer<int>(device);
        this.Vertices = new VertexBuffer<ModelVertex>(device);
        this.Primitives = Array.Empty<Primitive>();
        this.Materials = Array.Empty<Material>();
    }

    public VertexBuffer<ModelVertex> Vertices { get; protected set; }
    public IndexBuffer<int> Indices { get; protected set; }
    public Primitive[] Primitives { get; protected set; }
    public Material[] Materials { get; protected set; }

    public int PrimitiveCount => this.Primitives.Length;

    public virtual void Dispose()
    {
        this.Indices.Dispose();
        this.Vertices.Dispose();
    }
}
