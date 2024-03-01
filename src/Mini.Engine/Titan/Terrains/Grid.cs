using LibGame.Mathematics;

namespace Mini.Engine.Titan.Terrains;

public interface IReadOnlyGrid<T>
{
    public T this[int index] { get; }
    public T this[int column, int row] { get; }

    public int Columns { get; }
    public int Rows { get; }
    public int Count { get; }
}

public sealed class Grid<T> : IReadOnlyGrid<T>
{
    private readonly T[] Tiles;

    public Grid(T[] tiles, int columns, int rows)
    {
        this.Tiles = tiles;
        this.Columns = columns;
        this.Rows = rows;
    }

    public int Columns { get; }
    public int Rows { get; }
    public int Count => this.Tiles.Length;

    public T this[int index]
    {
        get => this.Tiles[index];
        set => this.Tiles[index] = value;
    }

    public T this[int column, int row]
    {
        get => this.Tiles[Indexes.ToOneDimensional(column, row, this.Columns)];
        set => this.Tiles[Indexes.ToOneDimensional(column, row, this.Columns)] = value;
    }

    public IReadOnlyGrid<T> AsReadOnly()
    {
        return this;
    }

    // TODO: create a method that allows us to take a slice of a grid, which is also a IReadOnlyGrid,
    // so that we can feed that transparently into other methods
}


