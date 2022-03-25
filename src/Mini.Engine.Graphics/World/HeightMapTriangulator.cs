using System;
using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

// based on https://mtnphil.wordpress.com/2012/10/15/terrain-triangulation-summary/
public static class HeightMapTriangulator
{
    public static (int[], ModelVertex[], BoundingBox bounds) Triangulate(float[] heightMap, int dimensions)
    {
        var width = ((dimensions - 1) * 2) + 1;
        var elements = width * width;        
        var positions = new Vector3[elements];

        // TODO: Multi thread, a simple Parallel.For doesn't make it faster, which of all these loops is the slowest part?
        for (var y = 0.0f; y <= dimensions - 1; y += 0.5f)        
        {            
            for (var x = 0.0f; x <= dimensions - 1; x += 0.5f)
            {
                var value = Sample(x, y, heightMap, dimensions);
                var pi = Indexes.ToOneDimensional((int)(x * 2), (int)(y * 2), width);
                positions[pi] = new Vector3(x, value, y);
            }
        }

        var indices = new int[(width - 1) * (width - 1) * 6];
        var ii = 0;
        for (var y = 0; y < width - 1; y++)
        {
            for (var x = 0; x < width - 1; x++)
            {
                var tl = Indexes.ToOneDimensional(x, y, width);
                var tr = Indexes.ToOneDimensional(x + 1, y, width);
                var br = Indexes.ToOneDimensional(x + 1, y + 1, width);
                var bl = Indexes.ToOneDimensional(x, y + 1, width);

                // TODO: do not use i++ but compute indices, then multi thread
                //var indexBase = y * (width - 1)
                // Choose the where to slice the quad into two triangles
                // so that for a 2x2 quad all diagonals connect to the center
                if ((x % 2 == 0) == (y % 2 == 0))
                {
                    indices[ii++] = tl;
                    indices[ii++] = tr;
                    indices[ii++] = br;

                    indices[ii++] = br;
                    indices[ii++] = bl;
                    indices[ii++] = tl;
                }
                else
                {
                    indices[ii++] = tr;
                    indices[ii++] = br;
                    indices[ii++] = bl;

                    indices[ii++] = bl;
                    indices[ii++] = tl;
                    indices[ii++] = tr;
                }
            }
        }


        var vertices = new ModelVertex[positions.Length];
        for (var vi = 0; vi < positions.Length; vi++)
        {
            var (x, y) = Indexes.ToTwoDimensional(vi, width);
            var texcoord = new Vector2(x / (float)width, y / (float)width);
            var position = positions[vi];
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

            vertices[vi]= new ModelVertex(position, texcoord, normal);
        }

        var bounds = ComputeBounds(vertices);

        return (indices, vertices, bounds);
    }

    private static BoundingBox ComputeBounds(IReadOnlyList<ModelVertex> vertices)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var minZ = float.MaxValue;

        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var maxZ = float.MinValue;

        for (var i = 0; i < vertices.Count; i++)
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
        return bounds;
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
}
