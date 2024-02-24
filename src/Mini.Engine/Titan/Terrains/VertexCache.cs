using LibGame.Mathematics;

namespace Mini.Engine.Titan.Terrains;
public sealed class VertexCache
{
    private readonly int Columns;
    private readonly int[][] Cache;

    public VertexCache(int columns, int rows)
    {
        this.Columns = columns;
        this.Vertices = [];
        this.Cache = new int[(columns + 1) * (rows + 1)][];
    }

    public List<TerrainVertex> Vertices { get; }

    public int AddVertex(Tile tile, TileCorner corner, int column, int row)
    {
        var (columnOffset, rowOffset) = GetCornerOffset(corner);

        var height = tile.GetHeight(corner);
        var index = Indexes.ToOneDimensional(column + columnOffset, row + rowOffset, this.Columns + 1);

        var array = this.Cache[index];

        // TODO: we can add evil unsafe code here that stores the index instead of the array pointer
        // if there's only one entry
        if (array == null)
        {
            this.Cache[index] = new int[1];
            return this.AddVertex(this.Cache[index], tile, corner, column, row);
        }

        for (var i = 0; i < array.Length; i++)
        {
            var v = array[i];
            if (this.Vertices[v].Position.Y == height)
            {
                return v;
            }
        }

        Array.Resize(ref array, array.Length + 1);
        return this.AddVertex(array, tile, corner, column, row);

    }

    public void Clear()
    {
        Array.Clear(this.Cache);
    }

    private int AddVertex(int[] indices, Tile tile, TileCorner corner, int column, int row)
    {
        var index = this.Vertices.Count;
        var position = TileUtilities.GetCornerPosition(column, row, tile, corner);
        this.Vertices.Add(new TerrainVertex(position));

        indices[^1] = index;

        return index;
    }

    private static (int columnOffset, int rowOffset) GetCornerOffset(TileCorner corner)
    {
        return corner switch
        {
            TileCorner.NE => (1, 0),
            TileCorner.SE => (1, 1),
            TileCorner.SW => (0, 1),
            TileCorner.NW => (0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(corner)),
        };
    }

}
