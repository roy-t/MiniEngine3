﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Mini.Engine.Core;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

// based on https://mtnphil.wordpress.com/2012/10/15/terrain-triangulation-summary/
public static class HeightMapTriangulator
{
    public static (int[], ModelVertex[], BoundingBox bounds) Triangulate(float[] heightMap, int stride)
    {
        // Create a vertex for ever point and half-way point in the heightMap so we can better follow the terrain
        // and have nicer normals
        var width = (stride * 2) - 1;

        var positionsTask = CalculatePositions(heightMap, width, stride);
        var indicesTask = CalculateIndices(width);
        
        positionsTask.Wait();
        var positions = positionsTask.Result;

        var verticesTask = CalculateVertices(positions, width);
        var boundsTask = CalculateBounds(positions);
        Task.WaitAll(indicesTask, verticesTask, boundsTask);

        var indices = indicesTask.Result;
        var vertices = verticesTask.Result;
        var bounds = boundsTask.Result;

        return (indices, vertices, bounds);
    }

    private async static Task<Vector3[]> CalculatePositions(float[] heightMap, int width, int stride)
    {
        var positions = new Vector3[width * width];
        await BackgroundTasks.ForAsync(0, width, y =>
        {
            for (var x = 0; x < width; x++)
            {
                var value = Sample(x / 2.0f, y / 2.0f, heightMap, stride);
                var pi = Indexes.ToOneDimensional(x, y, width);
                positions[pi] = new Vector3(x / 2.0f, value, y / 2.0f);
            }
        });

        return positions;
    }

    private async static Task<int[]> CalculateIndices(int width)
    {
        var intervals = width - 1;
        var quads = intervals * intervals;
        var triangles = quads * 2;
        var indices = new int[triangles * 3];
        await BackgroundTasks.ForAsync(0, intervals, y =>
        {
            for (var x = 0; x < intervals; x++)
            {
                var tl = Indexes.ToOneDimensional(x, y, width);
                var tr = Indexes.ToOneDimensional(x + 1, y, width);
                var br = Indexes.ToOneDimensional(x + 1, y + 1, width);
                var bl = Indexes.ToOneDimensional(x, y + 1, width);

                var indexBase = Indexes.ToOneDimensional(x, y, intervals) * 6;

                // Choose the where to slice the quad into two triangles
                // so that for a 2x2 quad all diagonals connect to the center
                if ((x % 2 == 0) == (y % 2 == 0))
                {
                    indices[indexBase + 0] = tl;
                    indices[indexBase + 1] = tr;
                    indices[indexBase + 2] = br;

                    indices[indexBase + 3] = br;
                    indices[indexBase + 4] = bl;
                    indices[indexBase + 5] = tl;
                }
                else
                {
                    indices[indexBase + 0] = tr;
                    indices[indexBase + 1] = br;
                    indices[indexBase + 2] = bl;

                    indices[indexBase + 3] = bl;
                    indices[indexBase + 4] = tl;
                    indices[indexBase + 5] = tr;
                }
            }
        });

        return indices;
    }

    private static async Task<ModelVertex[]> CalculateVertices(Vector3[] positions, int stride)
    {
        var vertices = new ModelVertex[positions.Length];

        await BackgroundTasks.ForAsync(0, positions.Length, vi =>
        {
            var (x, y) = Indexes.ToTwoDimensional(vi, stride);
            var texcoord = new Vector2(x / (float)stride, y / (float)stride);
            var position = positions[vi];
            var normal = Vector3.Zero;

            // TODO: what about the border?
            if (x > 0 && x < stride - 1 && y > 0 && y < stride - 1)
            {
                var xm = GetHeight(x - 1, y, positions, stride);
                var xp = GetHeight(x + 1, y, positions, stride);
                var ym = GetHeight(x, y - 1, positions, stride);
                var yp = GetHeight(x, y + 1, positions, stride);

                var B = new Vector3(1, xp - xm, 0);
                var T = new Vector3(0, yp - ym, 1);
                var N = Vector3.Cross(T, B);
                normal = Vector3.Normalize(N);
            }

            vertices[vi] = new ModelVertex(position, texcoord, normal);
        });

        return vertices;
    }

    private static Task<BoundingBox> CalculateBounds(Vector3[] positions)
    {
        return Task.Run(() =>
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var minZ = float.MaxValue;

            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxZ = float.MinValue;

            for (var i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                minX = Math.Min(minX, position.X);
                minY = Math.Min(minY, position.Y);
                minZ = Math.Min(minZ, position.Z);

                maxX = Math.Max(maxX, position.X);
                maxY = Math.Max(maxY, position.Y);
                maxZ = Math.Max(maxZ, position.Z);
            }

            var bounds = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
            return bounds;
        });
    }

    private static float Sample(float x, float y, float[] heightMap, int stride)
    {
        var xi = (int)x;
        var xxi = Math.Clamp((int)(x + 1.0f), 0, stride - 1);
        var yi = (int)y;
        var yyi = Math.Clamp((int)(y + 1.0f), 0, stride - 1);

        var fracix = x - MathF.Floor(x);
        var invFracix = 1.0f - fracix;
        var fraciy = y - MathF.Floor(y);
        var invFraciy = 1.0f - fraciy;

        var vxy = GetHeight(xi, yi, heightMap, stride);
        var vxxy = GetHeight(xxi, yi, heightMap, stride);
        var vxyy = GetHeight(xi, yyi, heightMap, stride);
        var vxxyy = GetHeight(xxi, yyi, heightMap, stride);

        var tl = (invFracix * invFraciy) * vxy;
        var tr = (fracix * invFraciy) * vxxy;
        var br = (fracix * fraciy) * vxxyy;
        var bl = (invFracix * fraciy) * vxyy;

        return tl + tr + br + bl;
    }

    private static float GetHeight(int x, int y, float[] heightMap, int stride)
    {
        return heightMap[Indexes.ToOneDimensional(x, y, stride)];
    }

    private static float GetHeight(int x, int y, IReadOnlyList<Vector3> vertices, int stride)
    {
        return vertices[Indexes.ToOneDimensional(x, y, stride)].Y;
    }
}
