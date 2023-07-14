using System.Diagnostics;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.Diesel.Tracks;

public readonly record struct ConnectedToReference(int XFrom, int YFrom, int IFrom, float UFrom, int XTo, int YTo, int ITo, float UTo);

public sealed class CurvePlacement2
{
    public CurvePlacement2(ICurve curve, Transform transform)
    {
        this.Curve = curve;
        this.Transform = transform;
    }

    public ICurve Curve { get; }
    public Transform Transform { get; }
}

public interface IReadOnlyCell
{
    IReadOnlyList<CurvePlacement2> Placements { get; }
}

public sealed class Cell : IReadOnlyCell
{
    public Cell(int capacity = 0)
    {
        this.Placements = new List<CurvePlacement2>(capacity);
    }

    public List<CurvePlacement2> Placements { get; }

    IReadOnlyList<CurvePlacement2> IReadOnlyCell.Placements => this.Placements;
}

public sealed class TrackGrid
{
    private static readonly IReadOnlyCell EmptyCell = new Cell();
    private readonly Cell[] Cells;

    private readonly float ApproximateDistanceFromCellBorderToCellCenter;
    private readonly float MaxValidConnectionDistanceSquared;

    public TrackGrid(int dimX, int dimY, int cellSizeX, int cellSizeY)
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
    public int CellSizeX { get; }
    public int CellSizeY { get; }

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

    public void AddPlacement(int x, int y, CurvePlacement2 placement)
    {
        var i = this.GetIndex(x, y);
        if (this.Cells[i] == null)
        {
            this.Cells[i] = new Cell(1);
        }

        this.Cells[i].Placements.Add(placement);
    }

    public void RemovePlacement(int x, int y, int index)
    {
        this.GetExistingCell(x, y).Placements.RemoveAt(index);
    }

    public void GetConnections(int x, int y, int index, float u, IList<ConnectedToReference> output)
    {
        // TODO: write tests for this method, but it looks promising!
        // TODO: make sure curves fit into grid!

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
                if (this.IsConnected(placement.Curve, u, placement.Transform, nextPlacement.Curve, 0.0f, nextPlacement.Transform))
                {
                    output.Add(new ConnectedToReference(x, y, index, u, ix, iy, i, 0.0f));
                }
                else if (this.IsConnected(placement.Curve, u, placement.Transform, nextPlacement.Curve, 1.0f, nextPlacement.Transform))
                {
                    output.Add(new ConnectedToReference(x, y, index, u, ix, iy, i, 1.0f));
                }
            }
        }
    }

    public (int x, int y) PickCell(Vector3 position)
    {
        var (ix, _, iz) = Grids.PickCell(this.DimX, 1, this.DimY, new Vector3(this.CellSizeX, 1.0f, this.CellSizeY), Vector3.Zero, position);
        return (ix, iz);
    }

    public bool IsConnected(ICurve a, float ua, Transform transformA, ICurve b, float ub, Transform transformB)
    {
        // Two curves are connected if the positions on their respective curves are close and their forwards either
        // point in exactly the same direction or in exactly the opposite direction
        var (positionA, forwardA) = a.GetWorldOrientation(ua, transformA);
        var (positionB, forwardB) = b.GetWorldOrientation(ub, transformB);

        if (Vector3.DistanceSquared(positionA, positionB) < this.MaxValidConnectionDistanceSquared)
        {
            return Math.Abs(Vector3.Dot(forwardA, forwardB)) > 0.95f;
        }

        return false;
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
