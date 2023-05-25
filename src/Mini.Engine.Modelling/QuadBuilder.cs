using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Graphics.Diesel;

namespace Mini.Engine.Modelling;

[Service]
public sealed class QuadBuilder
{
    private readonly Device Device;

    public QuadBuilder(Device device)
    {
        this.Device = device;
    }    

    public static ReadOnlySpan<PrimitiveVertex> GetVertices(ReadOnlySpan<Quad> quads)
    {
        var vertices = new ArrayBuilder<PrimitiveVertex>(quads.Length * 4);

        for (var q = 0; q < quads.Length; q++)
        {
            var quad = quads[q];

            var normal = quad.GetNormal();
            vertices.Add(new PrimitiveVertex(quad.A, normal));
            vertices.Add(new PrimitiveVertex(quad.B, normal));
            vertices.Add(new PrimitiveVertex(quad.C, normal));
            vertices.Add(new PrimitiveVertex(quad.D, normal));
        }

        return vertices.Build();
    }

    public static ReadOnlySpan<int> GetIndices(ReadOnlySpan<Quad> quads)
    {
        var indices = new ArrayBuilder<int>(quads.Length * 6);

        for (var q = 0; q < quads.Length; q++)
        {
            indices.Add((q * 4) + 0);
            indices.Add((q * 4) + 1);
            indices.Add((q * 4) + 2);
            indices.Add((q * 4) + 2);
            indices.Add((q * 4) + 3);
            indices.Add((q * 4) + 0);
        }

        return indices.Build();
    }

    public ILifetime<StructuredBuffer<Matrix4x4>> Instance(string name, params Matrix4x4[] instances)
    {
        var buffer = new StructuredBuffer<Matrix4x4>(this.Device, name);
        buffer.MapData(this.Device.ImmediateContext, instances);

        return this.Device.Resources.Add(buffer);
    }
}
