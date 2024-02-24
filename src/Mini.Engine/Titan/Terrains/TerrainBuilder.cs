using System.Numerics;
using LibGame.Graphics;
using LibGame.Mathematics;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Terrains;

public sealed class TerrainBuilder
{
    private readonly ColorLinear[] Palette;
    private readonly VertexCache Cache;
    private readonly ZoneOptimizer Optimizer;

    private readonly int Columns;
    private readonly int Rows;

    public TerrainBuilder(int columns, int rows)
    {
        this.Columns = columns;
        this.Rows = rows;

        this.Palette = new ColorLinear[byte.MaxValue];
        for (var i = 0; i < this.Palette.Length; i++)
        {
            var shade = i / (float)byte.MaxValue;
            this.Palette[i] = new ColorLinear(shade, shade, shade);
        }

        this.Cache = new VertexCache(columns, rows);
        this.Optimizer = new ZoneOptimizer(columns, rows);
        this.Indices = new List<int>();
        this.Triangles = new List<Triangle>();
    }

    public List<int> Indices { get; }
    public List<Triangle> Triangles { get; }
    public List<TerrainVertex> Vertices => this.Cache.Vertices;

    public void Update(IReadOnlyList<Tile> tiles)
    {
        this.Cache.Clear();
        this.Optimizer.Clear();
        this.Indices.Clear();
        this.Triangles.Clear();

        this.Optimizer.Optimize(tiles, this.Columns, this.Rows);

        var zones = this.Optimizer.Zones;
        for (var i = 0; i < zones.Count; i++)
        {
            var zone = zones[i];
            this.CreateRectangle(tiles, in zone);
        }

        this.AddCliffs(tiles);
    }

    private void CreateRectangle(IReadOnlyList<Tile> tiles, in Zone zone)
    {
        var north = zone.StartRow;
        var east = zone.EndColumn;
        var south = zone.EndRow;
        var west = zone.StartColumn;

        var ne = this.AddVertex(tiles, east, north, TileCorner.NE);
        var se = this.AddVertex(tiles, east, south, TileCorner.SE);
        var sw = this.AddVertex(tiles, west, south, TileCorner.SW);
        var nw = this.AddVertex(tiles, west, north, TileCorner.NW);

        var pNE = this.Cache.Vertices[ne].Position;
        var pSE = this.Cache.Vertices[se].Position;
        var pSW = this.Cache.Vertices[sw].Position;
        var pNW = this.Cache.Vertices[nw].Position;

        Vector3 n0;
        Vector3 n1;

        // Note: changing the triangulation should also change the BHV

        // XOO
        // OXO
        // OOX
        if (pSE.Y == pNW.Y)
        {
            this.Indices.Add(ne);
            this.Indices.Add(se);
            this.Indices.Add(nw);

            this.Indices.Add(se);
            this.Indices.Add(sw);
            this.Indices.Add(nw);

            n0 = LibGame.Geometry.Triangles.GetNormal(pNE, pSE, pNW);
            n1 = LibGame.Geometry.Triangles.GetNormal(pSE, pSW, pNW);
        }
        // OOX
        // OXO
        // XOO
        else
        {
            this.Indices.Add(ne);
            this.Indices.Add(se);
            this.Indices.Add(sw);

            this.Indices.Add(sw);
            this.Indices.Add(nw);
            this.Indices.Add(ne);

            n0 = LibGame.Geometry.Triangles.GetNormal(pNE, pSE, pSW);
            n1 = LibGame.Geometry.Triangles.GetNormal(pSW, pNW, pNE);
        }

        var y = Math.Max(pNE.Y, Math.Max(pSE.Y, Math.Max(pSW.Y, pNW.Y)));
        var albedo = this.Palette[(int)y];

        this.Triangles.Add(new Triangle() { Normal = n0, Albedo = albedo });
        this.Triangles.Add(new Triangle() { Normal = n1, Albedo = albedo });
    }

    private int AddVertex(IReadOnlyList<Tile> tiles, int column, int row, TileCorner corner)
    {
        var tile = tiles[Indexes.ToOneDimensional(column, row, this.Columns)];
        var index = this.Cache.AddVertex(tile, corner, column, row);

        return index;
    }

    public void AddCliffs(IReadOnlyList<Tile> tiles)
    {
        for (var it = 0; it < tiles.Count; it++)
        {
            var (x, y) = Indexes.ToTwoDimensional(it, this.Columns);
            if (x > 0)
            {
                this.AddCliff(tiles, it, TileSide.West);
            }

            if (x < this.Columns - 1)
            {
                this.AddCliff(tiles, it, TileSide.East);
            }

            if (y > 0)
            {
                this.AddCliff(tiles, it, TileSide.North);
            }

            if (y < this.Rows - 1)
            {
                this.AddCliff(tiles, it, TileSide.South);
            }
        }
    }

    private void AddCliff(IReadOnlyList<Tile> tiles, int index, TileSide side)
    {
        var (x, y) = Indexes.ToTwoDimensional(index, this.Columns);
        var (nx, ny) = TileUtilities.GetNeighbourIndex(x, y, side);

        // Note: variables are named as if neighbour is current's northern neighbour, but this function works for any neighbour/side

        var cTile = tiles[Indexes.ToOneDimensional(x, y, this.Columns)];
        var nTile = tiles[Indexes.ToOneDimensional(nx, ny, this.Columns)];

        (var cNWCorner, var cNECorner) = TileUtilities.TileSideToTileCorners(side);
        (var nSECorner, var nSWCorner) = TileUtilities.TileSideToTileCorners(TileUtilities.GetOppositeSide(side));

        var cNWHeight = cTile.GetHeight(cNWCorner);
        var cNEHeight = cTile.GetHeight(cNECorner);

        var nSEHeight = nTile.GetHeight(nSECorner);
        var nSWHeight = nTile.GetHeight(nSWCorner);

        // We only care about our sides being higher, the other situations will be taken care of by working on the other tile's sides
        if (cNWHeight > nSWHeight || cNEHeight > nSEHeight) // Cliff
        {
            var cNWIndex = this.Cache.AddVertex(cTile, cNWCorner, x, y);
            var cNEIndex = this.Cache.AddVertex(cTile, cNECorner, x, y);
            var nSEIndex = this.Cache.AddVertex(nTile, nSECorner, nx, ny);
            var nSWIndex = this.Cache.AddVertex(nTile, nSWCorner, nx, ny);

            var normal = side switch
            {
                TileSide.North => new Vector3(0.0f, 0.0f, -1.0f),
                TileSide.East => new Vector3(1.0f, 0.0f, 0.0f),
                TileSide.South => new Vector3(0.0f, 0.0f, 1.0f),
                TileSide.West => new Vector3(-1.0f, 0.0f, 0.0f),
                _ => throw new ArgumentOutOfRangeException(nameof(side)),
            };

            var albedo = Colors.RGBToLinear(new ColorRGB(0.5f, 0.25f, 0.20f));

            if (cNWHeight > nSWHeight && cNEHeight > nSEHeight) // A > C && B > D
            {
                this.Indices.EnsureCapacity(this.Indices.Count + 6);
                this.Indices.Add(cNWIndex);
                this.Indices.Add(nSWIndex);
                this.Indices.Add(nSEIndex);
                this.Indices.Add(nSEIndex);
                this.Indices.Add(cNEIndex);
                this.Indices.Add(cNWIndex);

                this.Triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
                this.Triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
            }
            else if (cNWHeight > nSWHeight) // A > C
            {
                this.Indices.EnsureCapacity(this.Indices.Count + 3);
                this.Indices.Add(cNWIndex);
                this.Indices.Add(nSWIndex);
                this.Indices.Add(nSEIndex);
                this.Triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
            }
            else if (cNEHeight > nSEHeight) // B > D
            {
                this.Indices.EnsureCapacity(this.Indices.Count + 3);
                this.Indices.Add(nSWIndex);
                this.Indices.Add(nSEIndex);
                this.Indices.Add(cNEIndex);
                this.Triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
            }
            else
            {
                throw new Exception("Unexpected case");
            }
        }
    }
}
