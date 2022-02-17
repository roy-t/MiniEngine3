using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models.Generators;

public static class CubeGenerator
{
    public static (int[], ModelVertex[]) Generate()
    {
        var vertices = new List<ModelVertex>(4 * 6);
        var indices = new List<int>(6 * 6);

        var right = new Vector3(1, 0, 0);
        var left = new Vector3(-1, 0, 0);
        var up = new Vector3(0, 1, 0);
        var down = new Vector3(0, -1, 0);
        var forward = new Vector3(0, 0, -1);
        var backward = new Vector3(0, 0, 1);

        // Front
        GenerateFace(new CoordinateSystem(right, up, backward), vertices, indices);

        // Back
        GenerateFace(new CoordinateSystem(left, up, forward), vertices, indices);

        // Left
        GenerateFace(new CoordinateSystem(backward, up, left), vertices, indices);

        // Right
        GenerateFace(new CoordinateSystem(forward, up, right), vertices, indices);

        // Top
        GenerateFace(new CoordinateSystem(right, forward, up), vertices, indices);

        // Botom
        GenerateFace(new CoordinateSystem(right, backward, down), vertices, indices);

        return (indices.ToArray(), vertices.ToArray());
    }

    public static IModel Generate(Device device, IMaterial material, string name)
    {
        (var indices, var vertices) = Generate();

        var bounds = new BoundingBox(-Vector3.One, Vector3.One);
        var primitives = new Primitive[]
        {
            new Primitive("Cube", bounds, 0, 0, indices.Length)
        };

        var materials = new IMaterial[] { material };
        return new Model(device, bounds, vertices, indices, primitives, materials, name);
    }

    private static void GenerateFace(CoordinateSystem coordinateSystem, List<ModelVertex> vertices, List<int> indices)
    {
        var maxX = coordinateSystem.UnitX;
        var maxY = coordinateSystem.UnitY;
        var maxZ = coordinateSystem.UnitZ;
        var normal = Vector3.Normalize(maxZ);

        var topLeft = -maxX + maxY + maxZ;
        var topRight = maxX + maxY + maxZ;
        var bottomRight = maxX - maxY + maxZ;
        var bottomLeft = -maxX - maxY + maxZ;

        var topLeftIndex = vertices.Count + 0;
        var topRightIndex = vertices.Count + 1;
        var bottomRightIndex = vertices.Count + 2;
        var bottomLeftIndex = vertices.Count + 3;

        vertices.Add(new ModelVertex(topLeft, new Vector2(0, 0), normal));
        vertices.Add(new ModelVertex(topRight, new Vector2(1, 0), normal));
        vertices.Add(new ModelVertex(bottomRight, new Vector2(1, 1), normal));
        vertices.Add(new ModelVertex(bottomLeft, new Vector2(0, 1), normal));

        indices.Add(topLeftIndex);
        indices.Add(topRightIndex);
        indices.Add(bottomRightIndex);

        indices.Add(bottomRightIndex);
        indices.Add(bottomLeftIndex);
        indices.Add(topLeftIndex);
    }
}
