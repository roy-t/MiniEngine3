using System.Diagnostics;
using System.Numerics;
using LibGame.Mathematics;
using Vortice.Mathematics;

namespace Mini.Engine.Titan.Terrains;
public sealed class TerrainBVH
{
    private readonly int Dimensions;
    private readonly byte[] Bounds;
    private readonly IReadOnlyList<Tile> Tiles;

    public TerrainBVH(IReadOnlyList<Tile> tiles, int columns, int rows)
    {
        if (columns != rows)
        {
            throw new NotSupportedException("The BVH requires that there are an equal number of columns and rows");
        }

        this.Tiles = tiles;
        this.Dimensions = columns;

        var totalSize = GetSize(columns);
        this.Bounds = new byte[totalSize];

        for (var i = 0; i < tiles.Count; i++)
        {
            this.Bounds[i] = Max(tiles[i]);
        }

        var offset = columns * columns;
        var dimensions = columns / 2;
        while (dimensions > 0)
        {
            var length = dimensions * dimensions;
            for (var i = 0; i < length; i++)
            {
                var (x, y) = Indexes.ToTwoDimensional(i, dimensions);

                var nw = this.GetHeight((x * 2) + 0, (y * 2) + 0, dimensions * 2);
                var ne = this.GetHeight((x * 2) + 1, (y * 2) + 0, dimensions * 2);
                var se = this.GetHeight((x * 2) + 1, (y * 2) + 1, dimensions * 2);
                var sw = this.GetHeight((x * 2) + 0, (y * 2) + 1, dimensions * 2);

                var max = Max(nw, ne, se, sw);
                this.Bounds[i + offset] = max;
            }

            offset += length;
            dimensions /= 2;
        }
    }

    public bool CheckTileHit(Ray ray, out int tileIndex, out Vector3 point)
    {
        if (this.CheckBHVHit(ray))
        {
            var best = this.CheckTileBoundingBoxHit(ray, 0, 0, 1);
            if (best.HasValue)
            {
                point = ray.Position + (ray.Direction * best.Value);
                var column = (int)point.X;
                var row = (int)point.Z;
                tileIndex = Indexes.ToOneDimensional(column, row, this.Dimensions);
                var tile = this.Tiles[tileIndex];
                var ne = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.NE);
                var se = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.SE);
                var sw = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.SW);
                var nw = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.NW);

                // Note: ray.Intersects expected a CW triangle, but uses OpenGL conventions (Z+ is forward)
                //       so we have to provide the arguments in CCW order.

                // Note: how we triangulate the tile depends on the shape of the tile, see TerrainBuilder

                // XOO
                // OXO
                // OOX
                if (se.Y == nw.Y)
                {
                    return ray.Intersects(in nw, in se, in ne, out point)
                        || ray.Intersects(in nw, in sw, in se, out point);
                }
                // OOX
                // OXO
                // XOO
                else
                {
                    return ray.Intersects(in sw, in se, in ne, out point)
                        || ray.Intersects(in ne, in nw, in sw, out point);
                }
            }
        }

        tileIndex = -1;
        point = Vector3.Zero;
        return false;
    }

    private float? CheckTileBoundingBoxHit(Ray ray, int column, int row, int dimensions)
    {
        var bounds = this.GetBounds(column, row, dimensions);
        var hit = ray.Intersects(bounds);
        if (hit.HasValue)
        {
            if (dimensions >= this.Dimensions)
            {
                var point = ray.Position + (ray.Direction * hit.Value);
                var c = Math.Clamp((int)point.X, 0, this.Dimensions - 1);
                var r = Math.Clamp((int)point.Z, 0, this.Dimensions - 1);
                var index = Indexes.ToOneDimensional(c, r, this.Dimensions);
                var tile = this.Tiles[index];
                var nep = TileUtilities.GetCornerPosition(c, r, tile, TileCorner.NE);
                var sep = TileUtilities.GetCornerPosition(c, r, tile, TileCorner.SE);
                var swp = TileUtilities.GetCornerPosition(c, r, tile, TileCorner.SW);
                var nwp = TileUtilities.GetCornerPosition(c, r, tile, TileCorner.NW);

                // Note: ray.Intersects expected a CW triangle, but uses OpenGL conventions (Z+ is forward)
                //       so we have to provide the arguments in CCW order.

                // Note: how we triangulate the tile depends on the shape of the tile, see TerrainBuilder

                // XOO
                // OXO
                // OOX
                if (sep.Y == nwp.Y)
                {
                    var detA = Intersects(in ray, in nwp, in sep, in nep).GetValueOrDefault(float.MaxValue);
                    var detB = Intersects(in ray, in nwp, in swp, in sep).GetValueOrDefault(float.MaxValue);
                    var detMin = Math.Min(detA, detB);
                    if (detMin != float.MaxValue)
                    {
                        return detMin;
                    }
                }
                // OOX
                // OXO
                // XOO
                else
                {
                    var detA = Intersects(in ray, in swp, in sep, in nep).GetValueOrDefault(float.MaxValue);
                    var detB = Intersects(in ray, in nep, in nwp, in swp).GetValueOrDefault(float.MaxValue);
                    var detMin = Math.Min(detA, detB);
                    if (detMin != float.MaxValue)
                    {
                        return detMin;
                    }
                }

                return null;
            }

            column *= 2;
            row *= 2;
            dimensions *= 2;
            var ne = this.CheckTileBoundingBoxHit(ray, column + 1, row + 0, dimensions).GetValueOrDefault(float.MaxValue);
            var se = this.CheckTileBoundingBoxHit(ray, column + 1, row + 1, dimensions).GetValueOrDefault(float.MaxValue);
            var sw = this.CheckTileBoundingBoxHit(ray, column + 0, row + 1, dimensions).GetValueOrDefault(float.MaxValue);
            var nw = this.CheckTileBoundingBoxHit(ray, column + 0, row + 0, dimensions).GetValueOrDefault(float.MaxValue);

            var best = Math.Min(ne, Math.Min(se, Math.Min(sw, nw)));
            if (best != float.MaxValue)
            {
                return best;
            }

        }

        return null;
    }

    private bool CheckBHVHit(Ray ray)
    {
        var bounds = this.GetBounds(0, 0, 1);
        return ray.Intersects(bounds).HasValue;
    }

    public byte GetHeight(int column, int row, int dimensions)
    {
        Debug.Assert(dimensions <= this.Dimensions);
        Debug.Assert(int.IsPow2(dimensions));

        var offset = this.Bounds.Length - GetSize(dimensions);
        return this.Bounds[offset + Indexes.ToOneDimensional(column, row, dimensions)];
    }


    public BoundingBox GetBounds(int column, int row, int dimensions)
    {
        // Note: a terrain with a single tile of height 1 would have the following bounds:
        // min: (0, 0, 0)
        // max: (1, 1, 1)
        var (startColum, endColumn, startRow, endRow) = this.GetCoverage(column, row, dimensions);

        var height = this.GetHeight(column, row, dimensions);
        var min = new Vector3(startColum, 0.0f, startRow);
        var max = new Vector3(endColumn + 1.0f, height, endRow + 1.0f);

        return new BoundingBox(min, max);
    }

    public (int startColumn, int endColumn, int startRow, int endRow) GetCoverage(int column, int row, int dimensions)
    {
        Debug.Assert(int.IsPow2(dimensions));
        Debug.Assert(dimensions <= this.Dimensions);
        Debug.Assert(column < dimensions);
        Debug.Assert(row < dimensions);

        // Say we start with dimensions := 4 inside a 32x32 terrain, we are sort of halway through the bvh
        // this means that a single element on this level covers 8x8 elements in the most detailed level
        // Log2(32/4) = Log2(8) = 3:  4->8, 8->16, 16->32, is doubling 3 times
        // 2 ^ 3 = 8, and indeed there are 4 8x8 patches in a 32x32 terrain
        var steps = MathF.Log2(this.Dimensions / dimensions);
        var span = (int)MathF.Pow(2, steps);
        return (column * span, ((column + 1) * span) - 1, (row * span), ((row + 1) * span) - 1);
    }

    private static int GetSize(int dimensions)
    {
        Debug.Assert(int.IsPow2(dimensions));

        return (int)((dimensions * dimensions) / (0.75f));
    }

    private static byte Max(Tile tile)
    {
        var (a, b, c, d) = tile.GetHeights();
        return Math.Max(a, Math.Max(b, Math.Max(c, d)));
    }

    private static byte Max(byte a, byte b, byte c, byte d)
    {
        return Math.Max(a, Math.Max(b, Math.Max(c, d)));
    }

    /// <summary>
    /// This does a ray cast on a triangle to see if there is an intersection.
    /// This ONLY works on CW wound triangles.
    /// </summary>
    /// <param name="v0">Triangle Corner 1</param>
    /// <param name="v1">Triangle Corner 2</param>
    /// <param name="v2">Triangle Corner 3</param>
    /// <param name="pointInTriangle">Intersection point if boolean returns true</param>
    /// <returns></returns>
    private static float? Intersects(in Ray ray, in Vector3 v0, in Vector3 v1, in Vector3 v2)
    {
        // Code origin can no longer be determined.
        // was adapted from C++ code.

        // compute normal
        var edgeA = v1 - v0;
        var edgeB = v2 - v0;

        var normal = Vector3.Cross(ray.Direction, edgeB);

        // find determinant
        var det = Vector3.Dot(edgeA, normal);

        // if perpendicular, exit
        if (det < MathHelper.NearZeroEpsilon)
        {
            return null;
        }
        det = 1.0f / det;

        // calculate distance from vertex0 to ray origin
        var s = ray.Position - v0;
        var u = det * Vector3.Dot(s, normal);

        if (u < -MathHelper.NearZeroEpsilon || u > 1.0f + MathHelper.NearZeroEpsilon)
        {
            return null;
        }

        var r = Vector3.Cross(s, edgeA);
        var v = det * Vector3.Dot(ray.Direction, r);
        if (v < -MathHelper.NearZeroEpsilon || u + v > 1.0f + MathHelper.NearZeroEpsilon)
        {
            return null;
        }

        // distance from ray to triangle
        det *= Vector3.Dot(edgeB, r);

        // Vector3 endPosition;
        // we dont want the point that is behind the ray cast.
        if (det < 0.0f)
        {
            return null;
        }

        return det;
    }
}
