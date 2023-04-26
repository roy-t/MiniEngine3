using System.Numerics;
using System.Xml.Linq;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel.Procedural;

public record struct Quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 Normal);

[Service]
public sealed class ProceduralMeshGenerator
{
    private readonly Device Device;

    public ProceduralMeshGenerator(Device device)
    {
        this.Device = device;
    }

    public ILifetime<PrimitiveMesh> GenerateQuad(Vector3 position, float extents, string name)
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

        var quad = new Quad(positions[0], positions[1], positions[2], positions[3], Vector3.UnitY);
        return this.FromQuads(name, quad);
    }

    private ILifetime<PrimitiveMesh> FromQuads(string name, params Quad[] quads)
    {
        var vertices = new PrimitiveVertex[quads.Length * 4];
        var indices = new int[quads.Length * 6];

        var nI = 0;
        var nV = 0;

        var bounds = BoundingBox.Empty;

        for (var i = 0; i < quads.Length; i++)
        {
            var quad = quads[i];

            indices[nI++] = nV + 0;
            indices[nI++] = nV + 1;
            indices[nI++] = nV + 2;

            indices[nI++] = nV + 2;
            indices[nI++] = nV + 3;
            indices[nI++] = nV + 0;

            vertices[nV++] = new PrimitiveVertex(quad.A, quad.Normal);
            vertices[nV++] = new PrimitiveVertex(quad.B, quad.Normal);
            vertices[nV++] = new PrimitiveVertex(quad.C, quad.Normal);
            vertices[nV++] = new PrimitiveVertex(quad.D, quad.Normal);

            bounds = BoundingBox.CreateMerged(bounds, BoundingBox.CreateFromPoints(new[] { quads[0].A, quads[0].B, quads[0].C, quads[0].D }));
        }

        var mesh = new PrimitiveMesh(this.Device, vertices, indices, bounds, name);
        return this.Device.Resources.Add(mesh);
    }
}
