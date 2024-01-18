using System.Numerics;
using LibGame.Geometry;

namespace Mini.Engine.Titan.Graphics;

public enum TileCorner : int
{
    NE = 0,
    SE = 1,
    SW = 2,
    NW = 3
}

public enum CornerType : byte
{
    Level,
    Raised,
    Lowered
}

public readonly record struct Neighbours<T>(T NW, T N, T NE, T W, T C, T E, T SW, T S, T SE)
    where T : struct
{ }

public readonly record struct TerrainTile(CornerType NE, CornerType SE, CornerType SW, CornerType NW, float Offset)
{
    public TerrainTile(float offset)
        : this(CornerType.Level, CornerType.Level, CornerType.Level, CornerType.Level, offset) { }

    public Vector4 GetHeightOffsets()
    {
        return new Vector4
        (
            TileUtilities.GetOffset(this.NE),
            TileUtilities.GetOffset(this.SE),
            TileUtilities.GetOffset(this.SW),
            TileUtilities.GetOffset(this.NW)
        );
    }

    public float GetHeightOffset(TileCorner corner)
    {
        return this.GetHeightOffsets()[(int)corner];
    }

    public Vector4 GetHeights()
    {
        return GetHeightOffsets() + Vector4.One * this.Offset;
    }

    public float GetHeight(TileCorner corner)
    {
        return this.GetHeights()[(int)corner];
    }
}

public static class TileUtilities
{
    public static float GetOffset(CornerType corner)
    {
        return corner switch
        {
            CornerType.Level => 0.0f,
            CornerType.Raised => 0.5f,
            CornerType.Lowered => -0.5f,
            _ => throw new ArgumentOutOfRangeException(nameof(corner)),
        };
    }

    public static Neighbours<T> GetNeighboursFromGrid<T>(int index, int stride, params T[] grid)
        where T : struct
    {
        var c = grid[index];
        var nw = GetFromGrid(grid, index, -(stride + 1), c);
        var n = GetFromGrid(grid, index, -stride, c);
        var ne = GetFromGrid(grid, index, -(stride - 1), c);
        var w = GetFromGrid(grid, index, -1, c);
        var e = GetFromGrid(grid, index, 1, c);
        var sw = GetFromGrid(grid, index, stride - 1, c);
        var s = GetFromGrid(grid, index, stride, c);
        var se = GetFromGrid(grid, index, stride + 1, c);

        return new Neighbours<T>(nw, n, ne, w, c, e, sw, s, se);
    }

    private static T GetFromGrid<T>(T[] grid, int index, int offset, T fallback)
        where T : struct
    {
        var actual = index + offset;
        if (actual >= 0 && actual < grid.Length)
        {
            return grid[actual];
        }

        return fallback;
    }

    public static TerrainTile FitNorth(TerrainTile n, float heightNorthWest, float heightNorthEast, float heightWest, float heightEast, float heightSouthWest, float heightSouth, float heightSouthEast, float baseHeight)
    {
        var hne = Fit(baseHeight, n.GetHeight(TileCorner.SE), heightNorthEast, heightEast);
        var hse = Fit(baseHeight, heightEast, heightSouthEast, heightSouth);
        var hsw = Fit(baseHeight, heightWest, heightSouthWest, heightSouth);
        var hnw = Fit(baseHeight, heightNorthWest, n.GetHeight(TileCorner.SW), heightWest);

        return new TerrainTile(hne, hse, hsw, hnw, baseHeight);
    }

    public static TerrainTile FitWest(TerrainTile w, float heightNorthWest, float heightNorth, float heightNorthEast, float heightEast, float heightSouthWest, float heightSouth, float heightSouthEast, float baseHeight)
    {
        var hne = Fit(baseHeight, heightNorth, heightNorthEast, heightEast);
        var hse = Fit(baseHeight, heightEast, heightSouthEast, heightSouth);
        var hsw = Fit(baseHeight, w.GetHeight(TileCorner.SE), heightSouthWest, heightSouth);
        var hnw = Fit(baseHeight, heightNorthWest, heightNorth, w.GetHeight(TileCorner.NE));

        return new TerrainTile(hne, hse, hsw, hnw, baseHeight);
    }


    public static TerrainTile Fit(TerrainTile nw, TerrainTile n, TerrainTile ne, TerrainTile w, float heightEast, float heightSouthWest, float heightSouth, float heightSouthEast, float baseHeight)
    {
        var hne = Fit(baseHeight, n.GetHeight(TileCorner.SE), ne.GetHeight(TileCorner.SW), heightEast);
        var hse = Fit(baseHeight, heightEast, heightSouthEast, heightSouth);
        var hsw = Fit(baseHeight, w.GetHeight(TileCorner.SE), heightSouthWest, heightSouth);
        var hnw = Fit(baseHeight, nw.GetHeight(TileCorner.SE), n.GetHeight(TileCorner.SW), w.GetHeight(TileCorner.NE));

        return new TerrainTile(hne, hse, hsw, hnw, baseHeight);
    }

    private static CornerType Fit(float baseHeight, params float[] options)
    {
        var result = baseHeight;
        for (var i = 0; i < options.Length; i++)
        {
            var height = options[i];
            if (IsWithin(height, baseHeight - 0.5f, baseHeight + 0.5f))
            {
                result = height;
                break;
            }
        }

        if (result > baseHeight)
        {
            return CornerType.Raised;
        }

        if (result < baseHeight)
        {
            return CornerType.Lowered;
        }

        return CornerType.Level;
    }

    private static bool IsWithin(float value, float min, float max, float error = 0.01f)
    {
        return value <= (max + error) || value >= (min - error);
    }

    public static Vector3 IndexToCorner(TerrainTile tile, TileCorner c)
    {
        var offset = tile.GetHeight(c);

        return c switch
        {
            TileCorner.NE => new Vector3(0.5f, offset, -0.5f),
            TileCorner.SE => new Vector3(0.5f, offset, 0.5f),
            TileCorner.SW => new Vector3(-0.5f, offset, 0.5f),
            TileCorner.NW => new Vector3(-0.5f, offset, -0.5f),
            _ => throw new IndexOutOfRangeException(),
        }; ;
    }

    public static Vector3 IndexToCorner(TileCorner c)
    {
        return c switch
        {
            TileCorner.NE => new Vector3(0.5f, 0.0f, -0.5f),
            TileCorner.SE => new Vector3(0.5f, 0.0f, 0.5f),
            TileCorner.SW => new Vector3(-0.5f, 0.0f, 0.5f),
            TileCorner.NW => new Vector3(-0.5f, 0.0f, -0.5f),
            _ => throw new IndexOutOfRangeException(),
        }; ;
    }


    public static (int a, int b, int c, int d, int e, int f) GetBestTriangleIndices(TerrainTile tile)
    {

        var offsets = tile.GetHeightOffsets();
        var ne = 0;
        var se = 1;
        var sw = 2;
        var nw = 3;

        // XOO
        // OXO
        // OOX
        if (offsets[1] == offsets[3])
        {
            return (ne, se, nw,
                    se, sw, nw);
        }
        // OOX
        // OXO
        // XOO
        else
        {
            return (ne, se, sw,
                    sw, nw, ne);
        }
    }

    /// <summary>
    /// Get the normals of the two triangles given the tile type and the
    /// relative indices (a, b, c) of the first triangle and (d, e, f) of the second triangle
    /// </summary>
    public static (Vector3 n0, Vector3 n1) GetNormals(TerrainTile tile, int a, int b, int c, int d, int e, int f)
    {
        var ca = IndexToCorner(tile, (TileCorner)a);
        var cb = IndexToCorner(tile, (TileCorner)b);
        var cc = IndexToCorner(tile, (TileCorner)c);
        var cd = IndexToCorner(tile, (TileCorner)d);
        var ce = IndexToCorner(tile, (TileCorner)e);
        var cf = IndexToCorner(tile, (TileCorner)f);

        var n0 = Triangles.GetNormal(ca, cb, cc);
        var n1 = Triangles.GetNormal(cd, ce, cf);

        return (n0, n1);
    }
}

