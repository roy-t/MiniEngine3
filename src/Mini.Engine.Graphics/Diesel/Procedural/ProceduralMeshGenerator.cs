using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel.Procedural;

public record struct Quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D);

[Service]
public sealed class ProceduralMeshGenerator
{
    public ILifetime<PrimitiveMesh> GenerateQuad(Device device, Vector3 position, float extents, string name)
    {
        var indices = new int[] { 0, 1, 2,
                                  2, 3, 0 };

        var positions = new Vector3[]
        {
            position + new Vector3(extents, 0, -extents), // NE
            position + new Vector3(extents, 0, extents), // SE
            position + new Vector3(-extents, 0, extents), // SW
            position + new Vector3(-extents, 0, -extents), // NW
        };

        var vertices = positions.Select(p => new PrimitiveVertex(p, Vector3.UnitY)).ToArray();

        var mesh = new PrimitiveMesh(device, vertices, indices, BoundingBox.CreateFromPoints(positions), name);

        return device.Resources.Add(mesh);
    }
}
