using System.Numerics;
using LibGame.Geometry;
using LibGame.Mathematics;

namespace Mini.Engine.Titan.Graphics;

public enum TileCorner : int
{
    NE = 0,
    SE = 1,
    SW = 2,
    NW = 3
}

public enum TileType : byte
{
    Flat,
    SlopeStartN,
    SlopeStartE,
    SlopeStartS,
    SlopeStartW,
    SlopeEndN,
    SlopeEndE,
    SlopeEndS,
    SlopeEndW,
    SlopeN,
    SlopeE,
    SlopeS,
    SlopeW,
    SlopeNE,
    SlopeSE,
    SlopeSW,
    SlopeNW,
}

public readonly record struct TerrainTile(TileType Type, float Offset)
{
    public Vector4 GetHeights()
    {
        return TileUtilities.GetCornerOffsets(this.Type) + (Vector4.One * this.Offset);
    }

    public float GetHeight(TileCorner corner)
    {
        return this.GetHeights()[(int)corner];
    }
}

public static class TileUtilities
{
    private static readonly Vector4 SlopeStartOffsets = new(0.5f, 0.0f, 0.0f, 0.5f);
    private static readonly Vector4 SlopeEndOffsets = new(0.0f, -0.5f, -0.5f, 0.0f);
    private static readonly Vector4 SlopeOffsets = new(0.5f, -0.5f, -0.5f, 0.5f);
    private static readonly Vector4 DiagonalSlopeOffsets = new(0.5f, 0.0f, -0.5f, 0.0f);


    public static List<TileType> Fit(TileCorner corner, float targetHeight, float baseHeight, List<TileType> options)
    {
        for (var i = options.Count - 1; i >= 0; i--)
        {
            var type = options[i];
            var height = baseHeight + GetCornerOffsets(type)[(int)corner];
            if (Math.Abs(targetHeight - height) > 0.01f)
            {
                options.RemoveAt(i);
            }
        }

        return options;
    }


    /// <summary>
    /// Returns the offsets (-1.0f, 0.0f, or 1.0f) of the 4 corners of the given tile
    /// The offets are returned in clockwise order, starting with the north-east corner
    /// </summary>
    public static Vector4 GetCornerOffsets(TileType type)
    {
        return type switch
        {
            TileType.Flat => Vector4.Zero,

            TileType.SlopeStartN => SlopeStartOffsets,
            TileType.SlopeStartE => SlopeStartOffsets.RotateRight(1),
            TileType.SlopeStartS => SlopeStartOffsets.RotateRight(2),
            TileType.SlopeStartW => SlopeStartOffsets.RotateRight(3),

            TileType.SlopeEndN => SlopeEndOffsets,
            TileType.SlopeEndE => SlopeEndOffsets.RotateRight(1),
            TileType.SlopeEndS => SlopeEndOffsets.RotateRight(2),
            TileType.SlopeEndW => SlopeEndOffsets.RotateRight(3),

            TileType.SlopeN => SlopeOffsets,
            TileType.SlopeE => SlopeOffsets.RotateRight(1),
            TileType.SlopeS => SlopeOffsets.RotateRight(2),
            TileType.SlopeW => SlopeOffsets.RotateRight(3),

            TileType.SlopeNE => DiagonalSlopeOffsets,
            TileType.SlopeSE => DiagonalSlopeOffsets.RotateRight(1),
            TileType.SlopeSW => DiagonalSlopeOffsets.RotateRight(2),
            TileType.SlopeNW => DiagonalSlopeOffsets.RotateRight(3),

            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    public static Vector3 IndexToCorner(TileType type, TileCorner c)
    {
        var offsets = GetCornerOffsets(type);

        return c switch
        {
            TileCorner.NE => new Vector3(0.5f, offsets[0], -0.5f),
            TileCorner.SE => new Vector3(0.5f, offsets[1], 0.5f),
            TileCorner.SW => new Vector3(-0.5f, offsets[2], 0.5f),
            TileCorner.NW => new Vector3(-0.5f, offsets[3], -0.5f),
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


    public static (int a, int b, int c, int d, int e, int f) GetBestTriangleIndices(TileType type)
    {
        var offsets = GetCornerOffsets(type);
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
    public static (Vector3 n0, Vector3 n1) GetNormals(TileType type, int a, int b, int c, int d, int e, int f)
    {
        var ca = IndexToCorner(type, (TileCorner)a);
        var cb = IndexToCorner(type, (TileCorner)b);
        var cc = IndexToCorner(type, (TileCorner)c);
        var cd = IndexToCorner(type, (TileCorner)d);
        var ce = IndexToCorner(type, (TileCorner)e);
        var cf = IndexToCorner(type, (TileCorner)f);

        var n0 = Triangles.GetNormal(ca, cb, cc);
        var n1 = Triangles.GetNormal(cd, ce, cf);

        return (n0, n1);
    }
}

