using System;
using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core;
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
    // based on https://mtnphil.wordpress.com/2012/10/15/terrain-triangulation-summary/
    public (int[], ModelVertex[]) Triangulate(float[] heightMap, int dimensions)
    {
        var indices = new List<int>();
        var positions = new List<Vector3>();
        var vertices = new List<ModelVertex>();

        for (var y= 0.0f; y <= dimensions - 1; y += 0.5f)
        {
            for (var x = 0.0f; x <= dimensions - 1; x += 0.5f)
            {
                var value = Sample(x, y, heightMap, dimensions);
                positions.Add(new Vector3(x, value, y));
            }
        }

        var width = (int)Math.Sqrt(positions.Count);

        for (var y = 0; y < width - 1; y++)
        {
            for (var x = 0; x < width - 1; x++)
            {
                var tl = Indexes.ToOneDimensional(x, y, width);
                var tr = Indexes.ToOneDimensional(x + 1, y, width);
                var br = Indexes.ToOneDimensional(x + 1, y + 1, width);
                var bl = Indexes.ToOneDimensional(x, y + 1, width);

                // Choose the where to slice the quad into two triangles
                // so that for a 2x2 quad all diagonals connect to the center
                if ((x % 2 == 0) == (y % 2 == 0))
                {
                    indices.Add(tl);
                    indices.Add(tr);
                    indices.Add(br);

                    indices.Add(br);
                    indices.Add(bl);
                    indices.Add(tl);
                }
                else
                {
                    indices.Add(tr);
                    indices.Add(br);
                    indices.Add(bl);

                    indices.Add(bl);
                    indices.Add(tl);
                    indices.Add(tr);
                }
            }
        }

        for (var i = 0; i < positions.Count; i++)
        {
            var (x, y) = Indexes.ToTwoDimensional(i, width);
            var texcoord = new Vector2(x / (float)width, y / (float)width);
            var position = positions[i];
            var normal = Vector3.Zero;            

            // TODO: what about the border?
            if (x > 0 && x < width - 1 && y > 0 && y < width - 1)
            {
                var xm = GetHeight(x - 1, y, positions, width);
                var xp = GetHeight(x + 1, y, positions, width);
                var ym = GetHeight(x, y - 1, positions, width);
                var yp = GetHeight(x, y + 1, positions, width);

                var B = new Vector3(1, xp - xm, 0);
                var T = new Vector3(0, yp - ym, 1);
                var N = Vector3.Cross(T, B);
                normal = Vector3.Normalize(N);
            }
            
            vertices.Add(new ModelVertex(position, texcoord, normal));
        }

        return (indices.ToArray(), vertices.ToArray());
    }

    private static float Sample(float x, float y, float[] heightMap, int dimensions)
    {
        var xi = (int)x;
        var xxi = Math.Clamp((int)(x + 1.0f), 0, dimensions - 1);
        var yi = (int)y;
        var yyi = Math.Clamp((int)(y + 1.0f), 0, dimensions - 1);

        var fracix = x - MathF.Floor(x);
        var invFracix = 1.0f - fracix;
        var fraciy = y - MathF.Floor(y);
        var invFraciy = 1.0f - fraciy;

        var vxy = GetHeight(xi, yi, heightMap, dimensions);
        var vxxy = GetHeight(xxi, yi, heightMap, dimensions);
        var vxyy = GetHeight(xi, yyi, heightMap, dimensions);
        var vxxyy = GetHeight(xxi, yyi, heightMap, dimensions);

        var tl = (invFracix * invFraciy) * vxy;
        var tr = (fracix * invFraciy) * vxxy;
        var br = (fracix * fraciy) * vxxyy;
        var bl = (invFracix * fraciy) * vxyy;

        return tl + tr + br + bl;
    }

    private static float GetHeight(int x, int y, float[] heightMap, int dimensions)
    {
        return heightMap[Indexes.ToOneDimensional(x, y, dimensions)];
    }

    private static float GetHeight(int x, int y, IReadOnlyList<Vector3> vertices, int dimensions)
    {
        return vertices[Indexes.ToOneDimensional(x, y, dimensions)].Y;
    }

    public IModel Triangulate(Device device, float[] heightMap, int dimensions, IMaterial material, string name)
    {
        (var indices, var vertices) = this.Triangulate(heightMap, dimensions);

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var minZ = float.MaxValue;

        var maxX = float.MinValue;        
        var maxY = float.MinValue;        
        var maxZ = float.MinValue;

        for(var i = 0; i < vertices.Length; i++)
        {
            var position = vertices[i].Position;
            minX = Math.Min(minX, position.X);
            minY = Math.Min(minY, position.Y);
            minZ = Math.Min(minZ, position.Z);

            maxX = Math.Max(maxX, position.X);            
            maxY = Math.Max(maxY, position.Y);
            maxZ = Math.Max(maxZ, position.Z);
        }

        var bounds = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        var primitives = new Primitive[]
        {
            new Primitive("HeightMap", bounds, 0, 0, indices.Length)
        };

        var materials = new IMaterial[] { material };
        return new Model(device, bounds, vertices, indices, primitives, materials, name);
    }
}
