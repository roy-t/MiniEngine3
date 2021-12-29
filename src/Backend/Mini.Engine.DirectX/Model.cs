using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Mini.Engine.DirectX.Resources;
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

public sealed record Primitive(string Name, int MaterialIndex, int IndexOffset, int IndexCount);

public class Material
{
    public Material(string name, ITexture2D albedo, ITexture2D metalicness, ITexture2D normal, ITexture2D roughness, ITexture2D ambientOcclusion)
    {
        this.Name = name;
        this.Albedo = albedo;
        this.Metalicness = metalicness;
        this.Normal = normal;
        this.Roughness = roughness;
        this.AmbientOcclusion = ambientOcclusion;
    }

    public string Name { get; }
    public ITexture2D Albedo { get; protected set; }
    public ITexture2D Metalicness { get; protected set; }
    public ITexture2D Normal { get; protected set; }
    public ITexture2D Roughness { get; protected set; }
    public ITexture2D AmbientOcclusion { get; protected set; }
}

public class Model : IDisposable
{
    public Model(Device device, ModelVertex[] vertices, int[] indices, Primitive[] primitives, Material[] materials, string name)
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
    public Material[] Materials { get; protected set; }

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
