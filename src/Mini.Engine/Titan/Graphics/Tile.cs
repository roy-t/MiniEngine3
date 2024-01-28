﻿using System.Numerics;
using LibGame.Geometry;

namespace Mini.Engine.Titan.Graphics;

public enum TileSide : byte
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}

public enum TileCorner : byte
{
    NE = 0,
    SE = 1,
    SW = 2,
    NW = 3
}

public enum CornerType : byte
{
    Level = 0,
    Raised = 1,
    Lowered = 2,
    Mask = 3
}

public readonly record struct Neighbours<T>(T NW, T N, T NE, T W, T C, T E, T SW, T S, T SE)
    where T : struct
{ }

// TODO: all this data can be stored in two bytes
public readonly struct Tile
{
    private const byte MaskNE = 0b_00_00_00_11;
    private const byte MaskSE = 0b_00_00_11_00;
    private const byte MaskSW = 0b_00_11_00_00;
    private const byte MaskNW = 0b_11_00_00_00;

    private readonly byte Offset;
    private readonly byte Corners;

    public Tile(CornerType ne, CornerType se, CornerType sw, CornerType nw, byte offset)
    {
        this.Offset = offset;

        byte corners = 0b_00_00_00_00;
        corners |= (byte)((int)ne << 0);
        corners |= (byte)((int)se << 2);
        corners |= (byte)((int)sw << 4);
        corners |= (byte)((int)nw << 6);

        this.Corners = corners;
    }

    public Tile(byte offset)
        : this(CornerType.Level, CornerType.Level, CornerType.Level, CornerType.Level, offset) { }

    public CornerType Unpack(TileCorner corner)
    {
        var ic = (int)corner;
        var mask = (byte)(MaskNE << (ic * 2));
        return (CornerType)((this.Corners & mask) >> (ic * 2));
    }

    public (CornerType NE, CornerType SE, CornerType SW, CornerType NW) UnpackAll()
    {
        var ne = (CornerType)((this.Corners & MaskNE) >> 0);
        var se = (CornerType)((this.Corners & MaskSE) >> 2);
        var sw = (CornerType)((this.Corners & MaskSW) >> 4);
        var nw = (CornerType)((this.Corners & MaskNW) >> 6);

        return (ne, se, sw, nw);
    }

    public sbyte GetHeightOffset(TileCorner corner)
    {
        return TileUtilities.GetOffset(this.Unpack(corner));
    }

    public (sbyte ne, sbyte se, sbyte sw, sbyte nw) GetHeightOffsets()
    {
        var (ne, se, sw, nw) = this.UnpackAll();
        return
        (
            TileUtilities.GetOffset(ne),
            TileUtilities.GetOffset(se),
            TileUtilities.GetOffset(sw),
            TileUtilities.GetOffset(nw)
        );
    }

    public byte GetHeight(TileCorner corner)
    {
        return (byte)(this.Offset + this.GetHeightOffset(corner));
    }

    public (byte ne, byte se, byte sw, byte nw) GetHeights()
    {
        var (ne, se, sw, nw) = this.GetHeightOffsets();
        return
        (
            (byte)(ne + this.Offset),
            (byte)(se + this.Offset),
            (byte)(sw + this.Offset),
            (byte)(nw + this.Offset)
        );
    }
}

public static class TileUtilities
{
    // TODO: a lot of these utilities can move to LibGame

    // Returns corners for each side from left to right
    public static (TileCorner A, TileCorner B) TileSideToTileCorners(TileSide side)
    {
        return side switch
        {
            TileSide.North => (TileCorner.NW, TileCorner.NE),
            TileSide.East => (TileCorner.NE, TileCorner.SE),
            TileSide.South => (TileCorner.SE, TileCorner.SW),
            TileSide.West => (TileCorner.SW, TileCorner.NW),
            _ => throw new ArgumentOutOfRangeException(nameof(side)),
        };
    }

    public static TileSide GetOppositeSide(TileSide side)
    {
        return side switch
        {
            TileSide.North => TileSide.South,
            TileSide.East => TileSide.West,
            TileSide.South => TileSide.North,
            TileSide.West => TileSide.East,
            _ => throw new ArgumentOutOfRangeException(nameof(side)),
        };
    }

    public static (int x, int y) GetNeighbourIndex(int x, int y, TileSide side)
    {
        return side switch
        {
            TileSide.North => (x + 0, y - 1),
            TileSide.East => (x + 1, y + 0),
            TileSide.South => (x + 0, y + 1),
            TileSide.West => (x - 1, y + 0),
            _ => throw new ArgumentOutOfRangeException(nameof(side))
        };
    }

    public static sbyte GetOffset(CornerType corner)
    {
        return corner switch
        {
            CornerType.Level => 0,
            CornerType.Raised => 1,
            CornerType.Lowered => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(corner)),
        };
    }

    // TODO: replace with a function that returns indexes instead of copies
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

    public static Tile FitNorth(Tile n, byte heightNorthWest, byte heightNorthEast, byte heightWest, byte heightEast, byte heightSouthWest, byte heightSouth, byte heightSouthEast, byte baseHeight)
    {
        var hne = Fit(baseHeight, n.GetHeight(TileCorner.SE), heightNorthEast, heightEast);
        var hse = Fit(baseHeight, heightEast, heightSouthEast, heightSouth);
        var hsw = Fit(baseHeight, heightWest, heightSouthWest, heightSouth);
        var hnw = Fit(baseHeight, heightNorthWest, n.GetHeight(TileCorner.SW), heightWest);

        return new Tile(hne, hse, hsw, hnw, baseHeight);
    }

    public static Tile FitWest(Tile w, byte heightNorthWest, byte heightNorth, byte heightNorthEast, byte heightEast, byte heightSouthWest, byte heightSouth, byte heightSouthEast, byte baseHeight)
    {
        var hne = Fit(baseHeight, heightNorth, heightNorthEast, heightEast);
        var hse = Fit(baseHeight, heightEast, heightSouthEast, heightSouth);
        var hsw = Fit(baseHeight, w.GetHeight(TileCorner.SE), heightSouthWest, heightSouth);
        var hnw = Fit(baseHeight, heightNorthWest, heightNorth, w.GetHeight(TileCorner.NE));

        return new Tile(hne, hse, hsw, hnw, baseHeight);
    }


    public static Tile Fit(Tile nw, Tile n, Tile ne, Tile w, byte heightEast, byte heightSouthWest, byte heightSouth, byte heightSouthEast, byte baseHeight)
    {
        var hne = Fit(baseHeight, n.GetHeight(TileCorner.SE), ne.GetHeight(TileCorner.SW), heightEast);
        var hse = Fit(baseHeight, heightEast, heightSouthEast, heightSouth);
        var hsw = Fit(baseHeight, w.GetHeight(TileCorner.SE), heightSouthWest, heightSouth);
        var hnw = Fit(baseHeight, nw.GetHeight(TileCorner.SE), n.GetHeight(TileCorner.SW), w.GetHeight(TileCorner.NE));

        return new Tile(hne, hse, hsw, hnw, baseHeight);
    }

    private static CornerType Fit(byte baseHeight, params byte[] options)
    {
        var result = baseHeight;
        for (var i = 0; i < options.Length; i++)
        {
            var height = options[i];
            if (IsWithin(height, baseHeight - 1, baseHeight + 1))
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

    private static bool IsWithin(int value, int min, int max)
    {
        return value <= max || value >= min;
    }

    public static Vector3 IndexToCorner(Tile tile, TileCorner c)
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

    // TODO: unused?
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


    public static (int a, int b, int c, int d, int e, int f) GetBestTriangleIndices(Tile tile)
    {

        var offsets = tile.GetHeightOffsets();
        var ne = 0;
        var se = 1;
        var sw = 2;
        var nw = 3;

        // XOO
        // OXO
        // OOX
        if (offsets.se == offsets.nw)
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
    public static (Vector3 n0, Vector3 n1) GetNormals(Tile tile, int a, int b, int c, int d, int e, int f)
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

