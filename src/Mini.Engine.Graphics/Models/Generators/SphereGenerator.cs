using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models.Generators;

public static class SphereGenerator
{
    private readonly record struct CoordinateSystem(Vector3 UnitX, Vector3 UnitY, Vector3 UnitZ);
    private readonly record struct Quad(int TopLeftIndex, int TopRightIndex, int BottomRightIndex, int BottomLeftIndex);

    public static IModel Generate(Device device, int subdivisions, IMaterial material, string name)
    {
        var vertices = new List<ModelVertex>();
        var indices = new List<int>();

        var right = new Vector3(1, 0, 0);
        var left = new Vector3(-1, 0, 0);
        var up = new Vector3(0, 1, 0);
        var down = new Vector3(0, -1, 0);
        var forward = new Vector3(0, 0, -1);
        var backward = new Vector3(0, 0, 1);

        // Front
        GenerateFace(new CoordinateSystem(right, up, backward), subdivisions, vertices, indices);

        // Back
        GenerateFace(new CoordinateSystem(left, up, forward), subdivisions, vertices, indices);

        // Left
        GenerateFace(new CoordinateSystem(backward, up, left), subdivisions, vertices, indices);

        // Right
        GenerateFace(new CoordinateSystem(forward, up, right), subdivisions, vertices, indices);

        // Top
        GenerateFace(new CoordinateSystem(right, forward, up), subdivisions, vertices, indices);

        // Botom
        GenerateFace(new CoordinateSystem(right, backward, down), subdivisions, vertices, indices);


        var primitives = new Primitive[]
        {
            new Primitive("Sphere", 0, 0, indices.Count)
        };

        var materials = new IMaterial[] { material };
        return new Model(device, vertices.ToArray(), indices.ToArray(), primitives, materials, name);
    }

    private static void GenerateFace(CoordinateSystem coordinateSystem, int subdivisions, List<ModelVertex> vertices, List<int> indices)
    {
        var quads = new List<Quad>();
        var currentIndex = indices.Append(-1).Max() + 1;

        var maxX = coordinateSystem.UnitX;
        var maxY = coordinateSystem.UnitY;
        var maxZ = coordinateSystem.UnitZ;

        var topLeft = -maxX + maxY + maxZ;
        var topRight = maxX + maxY + maxZ;
        var bottomRight = maxX - maxY + maxZ;
        var bottomLeft = -maxX - maxY + maxZ;

        var verticesPerEdge = subdivisions + 2;
        var indexLookup = new int[verticesPerEdge, verticesPerEdge];

        for (var column = 0; column < verticesPerEdge; column++)
        {
            for (var row = 0; row < verticesPerEdge; row++)
            {
                var x = FaceLerp(column / (verticesPerEdge - 1.0f));
                var y = FaceLerp(row / (verticesPerEdge - 1.0f));

                var centerLeft = Vector3.Lerp(topLeft, bottomLeft, y);
                var r = Vector3.Lerp(topRight, bottomRight, y);

                var position = Vector3.Normalize(Vector3.Lerp(centerLeft, r, x));
                var texture = new Vector2(x, y);
                vertices.Add(new ModelVertex(position, texture, position));

                indexLookup[column, row] = currentIndex++;

                if (column > 0 && row > 0)
                {
                    var topLeftIndex = indexLookup[column - 1, row - 1];
                    var topRightIndex = indexLookup[column, row - 1];
                    var bottomRightIndex = indexLookup[column, row];
                    var bottomLeftIndex = indexLookup[column - 1, row];

                    quads.Add(new Quad(topLeftIndex, topRightIndex, bottomRightIndex, bottomLeftIndex));
                }
            }
        }

        TriangulateFace(vertices, quads, indices);
    }

    private static float FaceLerp(float amount)
    {
        var angle = -MathHelper.PiOver4 + (amount * MathHelper.PiOver2);

        var basis = Vector3.UnitZ;
        var v0 = Vector3.Transform(basis, Matrix4x4.CreateRotationY(-MathHelper.PiOver4));
        var v1 = Vector3.Transform(basis, Matrix4x4.CreateRotationY(MathHelper.PiOver4));

        var target = Vector3.Normalize(Vector3.Transform(basis, Matrix4x4.CreateRotationY(angle)));        
        var plane = Plane.CreateFromVertices(v0, v1, v1 + Vector3.UnitY);
        var ray = new Ray(Vector3.Zero, target);
        var distance = ray.Intersects(plane).GetValueOrDefault();

        var position = ray.Position + (ray.Direction * distance);

        var fullLength = Vector3.Distance(v0, v1);
        var partialLength = Vector3.Distance(v0, position);

        return partialLength / fullLength;
    }

    private static void TriangulateFace(List<ModelVertex> vertices, List<Quad> quads, List<int> indices)
    {
        for (var i = 0; i < quads.Count; i++)
        {
            var quad = quads[i];
            var topLeftBottomRightDistance = Vector3.DistanceSquared(vertices[quad.TopLeftIndex].Position,
                vertices[quad.BottomRightIndex].Position);

            var topRightBottomLeftDistance = Vector3.DistanceSquared(vertices[quad.TopRightIndex].Position,
                vertices[quad.BottomLeftIndex].Position);

            if (topLeftBottomRightDistance < topRightBottomLeftDistance)
            {
                indices.Add(quad.TopLeftIndex);
                indices.Add(quad.TopRightIndex);
                indices.Add(quad.BottomRightIndex);

                indices.Add(quad.BottomRightIndex);
                indices.Add(quad.BottomLeftIndex);
                indices.Add(quad.TopLeftIndex);
            }
            else
            {
                indices.Add(quad.TopRightIndex);
                indices.Add(quad.BottomRightIndex);
                indices.Add(quad.BottomLeftIndex);

                indices.Add(quad.BottomLeftIndex);
                indices.Add(quad.TopLeftIndex);
                indices.Add(quad.TopRightIndex);
            }
        }
    }
}
