using System.Numerics;
using LibGame.Collections;
using LibGame.Graphics;
using LibGame.Mathematics;
using LibGame.Tiles;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Terrains;

public sealed class TerrainBuilder
{
    private readonly IReadOnlyGrid<Tile> Tiles;
    private readonly ColorLinear[] Palette;
    private readonly VertexCache Cache;
    private readonly ZoneOptimizer Optimizer;

    public TerrainBuilder(IReadOnlyGrid<Tile> tiles)
    {
        // TODO: use a more interesting palette
        this.Palette = new ColorLinear[byte.MaxValue];
        for (var i = 0; i < this.Palette.Length; i++)
        {
            var shade = i / (float)byte.MaxValue;
            this.Palette[i] = new ColorLinear(shade, shade, shade);
        }

        this.Cache = new VertexCache(tiles.Columns, tiles.Rows);
        this.Optimizer = new ZoneOptimizer(tiles.Columns, tiles.Rows);
        this.Indices = new List<int>();
        this.Triangles = new List<Triangle>();
        this.Tiles = tiles;
    }

    public List<int> Indices { get; }
    public List<Triangle> Triangles { get; }
    public List<TerrainVertex> Vertices => this.Cache.Vertices;

    /// <summary>
    /// Updates the render data for the terrain mesh. If cancelled the render data can be invalid
    /// </summary>
    public void Update(CancellationToken cancellationToken)
    {
        this.Cache.Clear();
        this.Optimizer.Clear();
        this.Indices.Clear();
        this.Triangles.Clear();

        this.Optimizer.Optimize(cancellationToken, this.Tiles);

        this.AddPlanes(cancellationToken);
        this.AddCliffs(cancellationToken);
    }

    private int AddVertex(IReadOnlyGrid<Tile> tiles, int column, int row, TileCorner corner)
    {
        var tile = tiles[column, row];
        var index = this.Cache.AddVertex(tile, corner, column, row);

        return index;
    }

    private void AddPlanes(CancellationToken cancellationToken)
    {
        var zones = this.Optimizer.Zones;
        for (var i = 0; i < zones.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var zone = zones[i];
            this.CreateRectangle(this.Tiles, in zone);
        }
    }

    private void AddCliffs(CancellationToken cancellationToken)
    {
        for (var it = 0; it < this.Tiles.Count; it++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var (column, row) = Indexes.ToTwoDimensional(it, this.Tiles.Columns);
            if (column > 0)
            {
                this.AddCliff(this.Tiles, column, row, TileSide.West);
            }

            if (column < this.Tiles.Columns - 1)
            {
                this.AddCliff(this.Tiles, column, row, TileSide.East);
            }

            if (row > 0)
            {
                this.AddCliff(this.Tiles, column, row, TileSide.North);
            }

            if (row < this.Tiles.Rows - 1)
            {
                this.AddCliff(this.Tiles, column, row, TileSide.South);
            }
        }
    }

    private void AddCliff(IReadOnlyGrid<Tile> tiles, int column, int row, TileSide side)
    {
        var (nx, ny) = tiles.GetNeighbourIndex(column, row, side);

        // Note: variables are named as if neighbour is current's northern neighbour, but this function works for any neighbour/side

        var cTile = tiles[column, row];
        var nTile = tiles[nx, ny];

        (var cNWCorner, var cNECorner) = TileUtilities.TileSideToTileCorners(side);
        (var nSECorner, var nSWCorner) = TileUtilities.TileSideToTileCorners(TileUtilities.GetOppositeSide(side));

        var cNWHeight = cTile.GetHeight(cNWCorner);
        var cNEHeight = cTile.GetHeight(cNECorner);

        var nSEHeight = nTile.GetHeight(nSECorner);
        var nSWHeight = nTile.GetHeight(nSWCorner);

        // We only care about our sides being higher, the other situations will be taken care of by working on the other tile's sides
        if (cNWHeight > nSWHeight || cNEHeight > nSEHeight) // Cliff
        {
            var cNWIndex = this.Cache.AddVertex(cTile, cNWCorner, column, row);
            var cNEIndex = this.Cache.AddVertex(cTile, cNECorner, column, row);
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

    private void CreateRectangle(IReadOnlyGrid<Tile> tiles, in Zone zone)
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
}
