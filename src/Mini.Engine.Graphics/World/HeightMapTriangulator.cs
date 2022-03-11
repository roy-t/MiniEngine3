using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class HeightMapTriangulator
{
    public HeightMapTriangulator()
    {

    }

    public (int[], ModelVertex[]) Triangulate(float[] heightMap, int dimensions)
    {
        var indices = new List<int>();
        var vertices = new List<ModelVertex>();

        return (indices.ToArray(), vertices.ToArray());
    }

    public IModel Triangulate(Device device, float[] heightMap, int dimensions, IMaterial material, string name)
    {
        (var indices, var vertices) = this.Triangulate(heightMap, dimensions);

        var bounds = new BoundingBox(-Vector3.One, Vector3.One);
        var primitives = new Primitive[]
        {
            new Primitive("Sphere", bounds, 0, 0, indices.Length)
        };

        var materials = new IMaterial[] { material };
        return new Model(device, bounds, vertices, indices, primitives, materials, name);
    }
}
