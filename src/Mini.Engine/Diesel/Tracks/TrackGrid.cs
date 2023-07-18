using System.Diagnostics;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.Diesel.Tracks;

public readonly record struct ConnectedToReference(int XFrom, int YFrom, int IFrom, float UFrom, int XTo, int YTo, int ITo, float UTo);

public sealed record class CurvePlacement(ICurve Curve, Transform Transform);

public interface IReadOnlyCell
{
    IReadOnlyList<CurvePlacement> Placements { get; }
}

public sealed class Cell : IReadOnlyCell
{
    public Cell(int capacity = 0)
    {
        this.Placements = new List<CurvePlacement>(capacity);
    }

    public List<CurvePlacement> Placements { get; }

    IReadOnlyList<CurvePlacement> IReadOnlyCell.Placements => this.Placements;
}

public sealed class TrackGrid
{
    private static readonly IReadOnlyCell EmptyCell = new Cell();
    private readonly Cell[] Cells;

    private readonly float ApproximateDistanceFromCellBorderToCellCenter;
    private readonly float MaxValidConnectionDistanceSquared;

    public TrackGrid(int dimX, int dimY, float cellSizeX, float cellSizeY)
    {
        this.DimX = dimX;
        this.DimY = dimY;

        this.CellSizeX = cellSizeX;
        this.CellSizeY = cellSizeY;
        this.ApproximateDistanceFromCellBorderToCellCenter = Math.Min(cellSizeX, cellSizeY) * 0.5f;
        this.MaxValidConnectionDistanceSquared = MathF.Pow(Math.Min(cellSizeX, cellSizeY) * 0.1f, 2.0f);

        this.Cells = new Cell[dimX * dimY];

    }

    public int DimX { get; }
    public int DimY { get; }
    public float CellSizeX { get; }
    public float CellSizeY { get; }

    public IReadOnlyCell this[int x, int y]
    {
        get
        {
            var i = this.GetIndex(x, y);
            if (this.Cells[i] != null)
            {
                return this.Cells[i];
            }

            return EmptyCell;
        }
    }

    public void Add(int x, int y, ICurve curve, Transform transform)
    {
        var i = this.GetIndex(x, y);
        if (this.Cells[i] == null)
        {
            this.Cells[i] = new Cell(1);
        }

        this.Cells[i].Placements.Add(new CurvePlacement(curve, transform));
    }

    public void Remove(int x, int y, int index)
    {
        this.GetExistingCell(x, y).Placements.RemoveAt(index);
    }

    public void GetConnections(int x, int y, int index, float u, IList<ConnectedToReference> output)
    {
        var placement = this.GetExistingCell(x, y).Placements[index];
        var (position, forward) = placement.Curve.GetWorldOrientation(u, placement.Transform);

        var positionInNextCell = position + (forward * this.ApproximateDistanceFromCellBorderToCellCenter);
        var (ix, iy) = this.PickCell(positionInNextCell);

        if (this.CellIsInsideGrid(ix, iy))
        {
            var cell = this[ix, iy];
            for (var i = 0; i < cell.Placements.Count; i++)
            {
                var nextPlacement = cell.Placements[i];
                if (placement.Curve.IsConnectedTo(u, placement.Transform, nextPlacement.Curve, 0.0f, nextPlacement.Transform, this.MaxValidConnectionDistanceSquared))
                {
                    output.Add(new ConnectedToReference(x, y, index, u, ix, iy, i, 0.0f));
                }
                else if (placement.Curve.IsConnectedTo(u, placement.Transform, nextPlacement.Curve, 1.0f, nextPlacement.Transform, MaxValidConnectionDistanceSquared))
                {
                    output.Add(new ConnectedToReference(x, y, index, u, ix, iy, i, 1.0f));
                }
            }
        }
    }

    public (int x, int y) PickCell(Vector3 position)
    {
        var (ix, _, iz) = Grids.PickCell(this.DimX, 1, this.DimY, new Vector3(this.CellSizeX, 0.0f, this.CellSizeY), Vector3.Zero, position);
        return (ix, iz);
    }

    public (Vector3 min, Vector3 max) GetCellBounds(int x, int y)
    {
        return Grids.GetCellBounds(this.DimX, 1, this.DimY, new Vector3(this.CellSizeX, 0.0f, this.CellSizeY), Vector3.Zero, x, 0, y);        
    }

    public Transform Add(ICurve curve, Vector3 approximatePosition, Vector3 forward)
    {
        // Find the cell the curve needs to be placed in
        var (x, y) = this.PickCell(approximatePosition);

        // Find a position on the border of the cell, backwards from the picked position
        var (cellMin, cellMax) = this.GetCellBounds(x, y);
        var midX = (cellMax.X + cellMin.X) / 2.0f;
        var midY = (cellMax.Y + cellMin.Y) / 2.0f;
        var midZ = (cellMax.Z + cellMin.Z) / 2.0f;

        Vector3 position;

        // Forward is pointing forward, start at the center of the 'backward' edge
        if (Vector3.Dot(forward, new Vector3(0, 0, -1)) > 0.95f)
        {
            position = new Vector3(midX, midY, cellMax.Z);
        }
        // Forward is pointing back, start at the center of the 'forward' edge
        else if (Vector3.Dot(forward, new Vector3(0, 0, 1)) > 0.95f)
        {
            position = new Vector3(midX, midY, cellMin.Z);
        }
        // Forward is pointing right, start at the center of 'left' edge
        else if (Vector3.Dot(forward, new Vector3(1, 0, 0)) > 0.95f)
        {
            position = new Vector3(cellMin.X, midY, midZ);
        }
        // Forward is pointing left, start at the center of 'right' edge
        else if (Vector3.Dot(forward, new Vector3(-1, 0, 0)) > 0.95f)
        {
            position = new Vector3(cellMax.X, midY, midZ);
        }
        else
        {
            throw new NotImplementedException("Unexpected direction");
        }

        var transform = curve.PlaceInXZPlane(0.0f, position, forward);

        this.Add(x, y, curve, transform);

        return transform;
    }

    private bool CellIsInsideGrid(int x, int y)
    {
        return x >= 0 && x < this.DimX && y >= 0 && y < this.DimY;
    }

    private int GetIndex(int x, int y)
    {
        Debug.Assert(this.CellIsInsideGrid(x, y));
        return Indexes.ToOneDimensional(x, y, this.DimX);
    }

    private Cell GetExistingCell(int x, int y)
    {
        var i = this.GetIndex(x, y);
        if (this.Cells[i] == null)
        {
            throw new NullReferenceException($"Invalid cell: {x}, {y}");
        }

        return this.Cells[i];
    }
}
