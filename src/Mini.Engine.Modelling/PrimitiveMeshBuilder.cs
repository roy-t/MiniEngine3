using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Diesel;
using Vortice.Mathematics;

namespace Mini.Engine.Modelling;

[Service]
public sealed class PrimitiveMeshBuilder
{
    private readonly Device Device;

    public PrimitiveMeshBuilder(Device device)
    {
        this.Device = device;
    }

    public ILifetime<PrimitiveMesh> FromQuads(string name, params Quad[] quads)
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
