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
            var column = 0;
            var row = 0;
            for (var dimension = 2; dimension <= this.Dimensions; dimension *= 2)
            {
                column *= 2;
                row *= 2;
                if (this.CheckTileHit(ray, column, row, dimension, out var hitColumn, out var hitRow))
                {
                    column = hitColumn;
                    row = hitRow;
                }
                else
                {
                    tileIndex = -1;
                    point = Vector3.Zero;
                    return false;
                }
            }

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
                return ray.Intersects(in nw, in se, in ne, out point) || ray.Intersects(in nw, in sw, in se, out point);
            }
            // OOX
            // OXO
            // XOO
            else
            {
                return ray.Intersects(in sw, in se, in ne, out point) || ray.Intersects(in ne, in nw, in sw, out point);
            }
        }

        tileIndex = -1;
        point = Vector3.Zero;
        return false;
    }


    private bool CheckBHVHit(Ray ray)
    {
        var bounds = this.GetBounds(0, 0, 1);
        return ray.Intersects(bounds).HasValue;
    }

    private bool CheckTileHit(Ray ray, int column, int row, int dimensions, out int hitColumn, out int hitRow)
    {
        var ne = this.GetBounds(column + 1, row + 0, dimensions);
        var se = this.GetBounds(column + 1, row + 1, dimensions);
        var sw = this.GetBounds(column + 0, row + 1, dimensions);
        var nw = this.GetBounds(column + 0, row + 0, dimensions);

        var best = float.MaxValue;
        hitColumn = 1;
        hitRow = 0;
        var intersectNorthEast = ray.Intersects(ne);
        if (intersectNorthEast.HasValue && intersectNorthEast.Value < best)
        {
            best = intersectNorthEast.Value;
            hitColumn = column + 1;
            hitRow = row + 0;
        }

        var intersectSouthEast = ray.Intersects(se);
        if (intersectSouthEast.HasValue && intersectSouthEast.Value < best)
        {
            best = intersectSouthEast.Value;
            hitColumn = column + 1;
            hitRow = row + 1;
        }

        var intersectSouthWest = ray.Intersects(sw);
        if (intersectSouthWest.HasValue && intersectSouthWest.Value < best)
        {
            best = intersectSouthWest.Value;
            hitColumn = column + 0;
            hitRow = row + 1;
        }

        var intersectNorthWest = ray.Intersects(nw);
        if (intersectNorthWest.HasValue && intersectNorthWest.Value < best)
        {
            best = intersectNorthWest.Value;
            hitColumn = column + 0;
            hitRow = row + 0;
        }

        return best != float.MaxValue;
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
