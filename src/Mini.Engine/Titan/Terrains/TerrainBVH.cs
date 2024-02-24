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
        if (this.CheckBVHHit(ray))
        {
            var best = this.CheckTileHit(ray, 0, 0, 1);
            if (best.HasValue)
            {
                point = ray.Position + (ray.Direction * best.Value);
                var column = Math.Clamp((int)point.X, 0, this.Dimensions - 1);
                var row = Math.Clamp((int)point.Z, 0, this.Dimensions - 1);
                tileIndex = Indexes.ToOneDimensional(column, row, this.Dimensions);
                return true;
            }
        }

        tileIndex = -1;
        point = Vector3.Zero;
        return false;
    }

    public bool CheckBVHHit(Ray ray)
    {
        var bounds = this.GetBounds(0, 0, 1);
        return ray.Intersects(bounds).HasValue;
    }

    private float? CheckTileHit(Ray ray, int column, int row, int dimensions)
    {
        var bounds = this.GetBounds(column, row, dimensions);
        var hit = ray.Intersects(bounds);
        if (hit.HasValue)
        {
            if (dimensions >= this.Dimensions)
            {
                return this.CheckTriangleHit(in ray, column, row);
            }

            column *= 2;
            row *= 2;
            dimensions *= 2;
            var ne = this.CheckTileHit(ray, column + 1, row + 0, dimensions).GetValueOrDefault(float.MaxValue);
            var se = this.CheckTileHit(ray, column + 1, row + 1, dimensions).GetValueOrDefault(float.MaxValue);
            var sw = this.CheckTileHit(ray, column + 0, row + 1, dimensions).GetValueOrDefault(float.MaxValue);
            var nw = this.CheckTileHit(ray, column + 0, row + 0, dimensions).GetValueOrDefault(float.MaxValue);

            var best = Math.Min(ne, Math.Min(se, Math.Min(sw, nw)));
            if (best != float.MaxValue)
            {
                return best;
            }
        }

        return null;
    }

    private float? CheckTriangleHit(in Ray ray, int column, int row)
    {
        var index = Indexes.ToOneDimensional(column, row, this.Dimensions);
        var tile = this.Tiles[index];
        var ne = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.NE);
        var se = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.SE);
        var sw = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.SW);
        var nw = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.NW);

        // Note: how we triangulate the tile depends on the shape of the tile, see TerrainBuilder

        // XOO
        // OXO
        // OOX
        if (se.Y == nw.Y)
        {
            var detA = Intersections.RayTriangle(in ray.Position, in ray.Direction, in ne, in se, in nw).GetValueOrDefault(float.MaxValue);
            var detB = Intersections.RayTriangle(in ray.Position, in ray.Direction, in se, in sw, in nw).GetValueOrDefault(float.MaxValue);
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
            var detA = Intersections.RayTriangle(in ray.Position, in ray.Direction, in ne, in se, in sw).GetValueOrDefault(float.MaxValue);
            var detB = Intersections.RayTriangle(in ray.Position, in ray.Direction, in sw, in nw, in ne).GetValueOrDefault(float.MaxValue);
            var detMin = Math.Min(detA, detB);
            if (detMin != float.MaxValue)
            {
                return detMin;
            }
        }

        return null;
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


}
