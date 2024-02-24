using System.Diagnostics;
using System.Numerics;
using LibGame.Mathematics;
using Vortice.Mathematics;

namespace Mini.Engine.Titan.Terrains;
public sealed class TerrainBVH
{
    private readonly int Dimensions;
    private readonly byte[] Bounds;

    public TerrainBVH(IReadOnlyList<Tile> tiles, int columns, int rows)
    {
        if (columns != rows)
        {
            throw new NotSupportedException("The BVH requires that there are an equal number of columns and rows");
        }

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

    private (int startColumn, int endColumn, int startRow, int endRow) GetCoverage(int column, int row, int dimensions)
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
        return (column, column + span - 1, row, row + span - 1);
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
