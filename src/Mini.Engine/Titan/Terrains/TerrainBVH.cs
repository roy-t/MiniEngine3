using System.Numerics;
using LibGame.Mathematics;
using Vortice.Mathematics;

namespace Mini.Engine.Titan.Terrains;
/// <summary>
/// This class creates a bounding volume hierarchy for a tile based terrain.
/// It stores the minimum and maximum height for each part of the hiearchy in a single array.
///
/// With this class it is easy, and efficient, to figure out exactly which tile is hit by a ray. For example,
/// a ray casted by the cursor, for when a user tries to select a tile.
/// </summary>
public sealed class TerrainBVH
{
    private readonly record struct MinMax(byte Min, byte Max);

    private readonly int Dimensions;
    private readonly MinMax[] Bounds;
    private readonly IReadOnlyGrid<Tile> Tiles;

    public TerrainBVH(IReadOnlyGrid<Tile> tiles)
    {
        if (tiles.Columns != tiles.Rows)
        {
            throw new NotSupportedException("This BVH works only on square terrains, columns and rows should be of equal length");
        }

        if (!int.IsPow2(tiles.Columns))
        {
            throw new NotSupportedException("This BVH works only if both dimensions are a power of 2");
        }

        this.Tiles = tiles;
        this.Dimensions = tiles.Columns;

        var totalLength = GetLength(this.Dimensions);
        this.Bounds = new MinMax[totalLength];

        // The first level in the hierarchy stores the minimum and maximum height of every single tile
        for (var i = 0; i < tiles.Count; i++)
        {
            this.Bounds[i] = GetMinMax(tiles[i]);
        }

        // Every next level stores the minimum and maximum height of four elements in the level below it.
        // The highest level is a single element that stores the minimum and maximum height of the entire terrain.
        var offset = this.Dimensions * this.Dimensions;
        var dimensions = this.Dimensions / 2;
        while (dimensions > 0)
        {
            var length = dimensions * dimensions;
            for (var i = 0; i < length; i++)
            {
                var (x, y) = Indexes.ToTwoDimensional(i, dimensions);

                var nw = this.GetMinMax((x * 2) + 0, (y * 2) + 0, dimensions * 2);
                var ne = this.GetMinMax((x * 2) + 1, (y * 2) + 0, dimensions * 2);
                var se = this.GetMinMax((x * 2) + 1, (y * 2) + 1, dimensions * 2);
                var sw = this.GetMinMax((x * 2) + 0, (y * 2) + 1, dimensions * 2);

                this.Bounds[i + offset] = GetMinMax(nw, ne, se, sw);
            }

            offset += length;
            dimensions /= 2;
        }
    }

    /// <summary>
    /// Conservatively updates the entire BVH hierarchy for the tile at the given position.
    /// Using this method will only make the BVH 'grow' as for shrinking we would need to
    /// check every tile.
    /// </summary>
    public void Update(int column, int row)
    {
        var offset = 0;
        var dimensions = this.Dimensions;

        var tile = this.Tiles[column, row];
        var (min, max) = GetMinMax(tile);

        while (dimensions > 0)
        {
            var i = Indexes.ToOneDimensional(column, row, dimensions);

            var (cmin, cmax) = this.Bounds[i + offset];
            this.Bounds[i + offset] = new MinMax(Math.Min(min, cmin), Math.Max(max, cmax));

            offset += dimensions * dimensions;
            column /= 2;
            row /= 2;
            dimensions /= 2;
        }
    }

    /// <summary>
    /// Computes the one dimensional index of the tile hit and the exact point in 3D space where the hit occured
    /// </summary>    
    public bool CheckTileHit(Ray ray, out int tileIndex, out Vector3 point)
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

        tileIndex = -1;
        point = Vector3.Zero;
        return false;
    }

    /// <summary>
    /// Check where a hit occurs by recursively walking the BVH.
    /// </summary>    
    private float? CheckTileHit(Ray ray, int column, int row, int dimensions)
    {
        // Check if the ray intersects with boundaries of the current level
        var bounds = this.GetBounds(column, row, dimensions);
        var hit = ray.Intersects(bounds);
        if (hit.HasValue)
        {
            // After the lowest level, check if the ray hits one of the two triangles that make-up the tile.
            if (dimensions >= this.Dimensions)
            {
                return this.CheckTriangleHit(in ray, column, row);
            }

            // If there is a hit, recursively check the four lower levels. Note that the ray can intersect
            // with more than one of these, so we need to drill all the way down and then select the
            // hit that appeared closest to the ray.
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

    /// <summary>
    /// Check if the ray intersects with one of the two triangles that make-up the tile in the given position
    /// </summary>    
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


    /// <summary>
    /// Computes a bounding box for the element in the given level of the BVH.
    /// If the terrain consists of 64x64 tiles. Calling this method with (3, 3, 4) computes
    /// the bounding box of the most south east element at the level where the BVH has 4x4
    /// elements. The corresponding bounding box would cover the 16x16 most south east tiles.
    /// </summary>    
    private BoundingBox GetBounds(int column, int row, int dimensions)
    {
        // Note: a terrain with a single flat tile of height 1 would have the following bounds:
        // min: (0, 1, 0)
        // max: (1, 1, 1)
        var (startColum, endColumn, startRow, endRow) = this.GetCoverage(column, row, dimensions);

        var minMax = this.GetMinMax(column, row, dimensions);
        var min = new Vector3(startColum, minMax.Min, startRow);
        var max = new Vector3(endColumn + 1.0f, minMax.Max, endRow + 1.0f);

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Computes which tiles a single element covers at this level in the BVH.
    /// If the terrain consists of 64x64 tiles. Calling this method with (3, 3, 4) computes
    /// the coluimns and rows of the most south east element at the level where the BVH has 4x4
    /// elements. The corresponding columns and rows would span the 16x16 most south east tiles.
    /// </summary>    
    private (int startColumn, int endColumn, int startRow, int endRow) GetCoverage(int column, int row, int dimensions)
    {
        // Say we start with dimensions := 4 inside a 32x32 terrain, we are sort of halway through the bvh
        // this means that a single element on this level covers 8x8 elements in the most detailed level
        // Log2(32/4) = Log2(8) = 3:  4->8, 8->16, 16->32, is doubling 3 times
        // 2 ^ 3 = 8, and indeed there are 4 8x8 patches in a 32x32 terrain
        var steps = MathF.Log2(this.Dimensions / dimensions);
        var span = (int)MathF.Pow(2, steps);
        return (column * span, ((column + 1) * span) - 1, (row * span), ((row + 1) * span) - 1);
    }

    private MinMax GetMinMax(int column, int row, int dimensions)
    {
        var offset = this.Bounds.Length - GetLength(dimensions);
        return this.Bounds[offset + Indexes.ToOneDimensional(column, row, dimensions)];
    }

    private static MinMax GetMinMax(Tile tile)
    {
        var (a, b, c, d) = tile.GetHeights();
        var min = Math.Min(a, Math.Min(b, Math.Min(c, d)));
        var max = Math.Max(a, Math.Max(b, Math.Max(c, d)));
        return new MinMax(min, max);
    }

    private static MinMax GetMinMax(MinMax a, MinMax b, MinMax c, MinMax d)
    {
        var min = Math.Min(a.Min, Math.Min(b.Min, Math.Min(c.Min, d.Min)));
        var max = Math.Max(a.Max, Math.Max(b.Max, Math.Max(c.Max, d.Max)));

        return new MinMax(min, max);
    }

    private static int GetLength(int dimensions)
    {
        return (int)((dimensions * dimensions) / (0.75f));
    }
}
