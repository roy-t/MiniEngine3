using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel.Procedural;

public record struct Shape(params Vector2[] Vertices);

public record struct Quad(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C, Vector3 D);

// TODO: bad name, not much procedural about it
// move all the shape/line math somewhere else
// move all the actions somewhere else

[Service]
public sealed class ProceduralMeshGenerator
{
    private readonly Device Device;

    public ProceduralMeshGenerator(Device device)
    {
        this.Device = device;
    }

    private static Shape CreateSingleRailCrossSection()
    {
        var railTopWidth = 0.1f;
        var railBottomWidth = 0.2f;
        var railHeigth = 0.2f;

        return new Shape(new Vector2(railTopWidth / 2.0f, railHeigth), new Vector2(railBottomWidth / 2.0f, 0.0f), new Vector2(-railBottomWidth / 2.0f, 0.0f), new Vector2(-railTopWidth / 2.0f, railHeigth));
    }

    private static Quad[] Extrude(Shape crossSection, float depth)
    {
        if (crossSection.Vertices.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        var quads = new Quad[crossSection.Vertices.Length];

        for (var i = 0; i < crossSection.Vertices.Length; i++)
        {
            var a = crossSection.Vertices[i];
            var b = crossSection.Vertices[(i + 1) % crossSection.Vertices.Length];
            var n = GetNormalFromLineSegement(a, b);
            var normal = new Vector3(n.X, n.Y, 0.0f);
            quads[i] = new Quad(normal, new Vector3(b.X, b.Y, -depth), new Vector3(b.X, b.Y, 0.0f), new Vector3(a.X, a.Y, 0.0f), new Vector3(a.X, a.Y, -depth));
        }

        return quads;
    }

    private static Vector2 GetNormalFromLineSegement(Vector2 start, Vector2 end)
    {
        Debug.Assert(start != end, $"{start} == {end}");

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;

        return Vector2.Normalize(new Vector2(-dy, dx));
    }

    public ILifetime<PrimitiveMesh> GenerateRail(string name)
    {
        var crossSection = CreateSingleRailCrossSection();
        var quads = Extrude(crossSection, 10.0f);
        return this.FromQuads(name, quads);
    }

    public ILifetime<PrimitiveMesh> GenerateQuad(Vector3 position, float extents, string name)
    {
        var positions = new Vector3[]
        {
            position + new Vector3(extents, 0, -extents), // NE
            position + new Vector3(extents, 0, extents), // SE
            position + new Vector3(-extents, 0, extents), // SW
            position + new Vector3(-extents, 0, -extents), // NW
        };

        var quad = new Quad(Vector3.UnitY, positions[0], positions[1], positions[2], positions[3]);
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

            bounds = BoundingBox.CreateMerged(bounds, BoundingBox.CreateFromPoints(new[] { quads[i].A, quads[i].B, quads[i].C, quads[i].D }));
        }

        var mesh = new PrimitiveMesh(this.Device, vertices, indices, bounds, name);
        return this.Device.Resources.Add(mesh);
    }
}
