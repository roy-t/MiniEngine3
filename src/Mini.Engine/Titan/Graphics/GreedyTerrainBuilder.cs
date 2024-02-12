using System.Numerics;
using LibGame.Geometry;
using LibGame.Mathematics;
using Mini.Engine.Configuration;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Graphics;

[Service]
public sealed class GreedyTerrainBuilder
{
    private readonly int[] vertexMap;
    private readonly List<TerrainVertex> vertices;
    private readonly List<int> indices;
    private readonly List<Triangle> triangles;

    private readonly int columns;
    private readonly int rows;

    public GreedyTerrainBuilder(int columns, int rows)
    {
        this.columns = columns;
        this.rows = rows;

        this.vertexMap = new int[(columns + 1) * (rows + 1)];
        this.vertices = new List<TerrainVertex>();
        this.indices = new List<int>();
        this.triangles = new List<Triangle>();
    }

    public TerrainMesh Build(IReadOnlyList<Tile> tiles)
    {
        Array.Clear(this.vertexMap);
        this.vertices.Clear();
        this.indices.Clear();
        this.triangles.Clear();

        var (_, zones) = ZoneOptimizer.Optimize(tiles, this.columns, this.rows);

        for (var i = 0; i < zones.Count; i++)
        {
            var zone = zones[i];
            this.CreateRectangle(tiles, in zone);
        }
    }

    private void CreateRectangle(IReadOnlyList<Tile> tiles, in Zone zone)
    {
        var n = zone.StartRow;
        var e = zone.EndColumn;
        var s = zone.EndRow;
        var w = zone.StartColumn;

        // What about get best triangle offset?

        var a = this.AddVertex(tiles, e, n, TileCorner.NE);
        var b = this.AddVertex(tiles, e, s, TileCorner.SE);
        var c = this.AddVertex(tiles, w, s, TileCorner.SW);

        var d = this.AddVertex(tiles, w, s, TileCorner.SW);
        var e = this.AddVertex(tiles, w, n, TileCorner.NW);
        var f = this.AddVertex(tiles, e, n, TileCorner.NE);

        var n0 = Triangles.GetNormal(this.vertices[a].Position, this.vertices[b].Position, this.vertices[c].Position);
        var n1 = Triangles.GetNormal(this.vertices[d].Position, this.vertices[e].Position, this.vertices[f].Position);

        // TODO: get color
        this.triangles.Add(new Triangle() { Normal = n0, Albedo = ...});
        this.triangles.Add(new Triangle() { Normal = n1, Albedo = ...});
    }

    private int AddVertex(IReadOnlyList<Tile> tiles, int column, int row, TileCorner corner)
    {
        var (columnOffset, rowOffset) = corner switch
        {
            TileCorner.NE => (1, 0),
            TileCorner.SE => (1, 1),
            TileCorner.SW => (0, 1),
            TileCorner.NW => (0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(corner)),
        };

        var i = Indexes.ToOneDimensional(column + columnOffset, row + rowOffset, this.columns + 1);
        if (this.vertexMap[i] != 0)
        {
            // We remove 1 since we use 0 to signal the map is unset
            var index = this.vertexMap[i] - 1;
            this.indices.Add(index);

            return index;
        }
        else
        {
            var tile = tiles[Indexes.ToOneDimensional(column, row, this.columns)];
            var position = this.GetTileCornerPosition(tile, corner, column, row);
            var vertex = new TerrainVertex(position);
            var index = this.vertices.Count;
            this.vertices.Add(vertex);

            // We add 1 since we use 0 to signal the map is unset
            this.vertexMap[i] = index + 1;
            this.indices.Add(index);

            return index;
        }
    }

    private Vector3 GetTileCornerPosition(Tile tile, TileCorner corner, int tileX, int tileY)
    {
        var offset = TileUtilities.IndexToCorner(tile, corner);
        return new Vector3(offset.X + tileX, offset.Y, offset.Z + tileY);
    }
}
