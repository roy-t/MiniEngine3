using System.Numerics;
using System.Runtime.InteropServices;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Diesel.v2.Primitives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Material(Vector3 Albedo, float Metalicness, float Roughness);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Part(uint Offset, uint Length, Material Material);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Vertex(Vector3 Position, Vector3 Normal)
{
    public static readonly InputElementDescription[] Elements =
    [
        new("POSITION", 0, Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
        new("NORMAL", 0, Format.R32G32B32_Float, 3 * sizeof(float), 0, InputClassification.PerVertexData, 0),
    ];
}

[System.Runtime.CompilerServices.InlineArray(MaxPartLength)]
public struct Buffer
{
    public const int MaxPartLength = 4;
    public Part[] Parts;
}

public readonly record struct Primitive(ILifetime<IndexBuffer<short>> Indices, ILifetime<VertexBuffer<Vertex>> Vertices, Buffer Parts);
