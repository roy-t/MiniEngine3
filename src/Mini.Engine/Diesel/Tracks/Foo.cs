using System.CodeDom;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.Diesel.Tracks;

[Service]
public sealed class Foo
{
    private readonly TrackGrid Grid;
    private readonly TrackManager TrackManager;

    public Foo()
    {
        this.Grid = new TrackGrid(100, 100, TrackParameters.STRAIGHT_LENGTH, TrackParameters.STRAIGHT_LENGTH);
    }

    public void Place(ICurve curve, Vector3 approximatePosition, Vector3 forward)
    {
        // Find the cell the curve needs to be placed in
        var (x, y) = this.Grid.PickCell(approximatePosition);

        // Find a position on the border of the cell, backwards from the picked position
        var (cellMin, cellMax) = this.Grid.GetCellBounds(x, y);
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

        this.Grid.Add(x, y, curve, transform);

        this.TrackManager.Add(curve, transform);

        // TODO: store some ID in the grid that makes it easy to remove the instance again
        // or alternatively we can just look at the equals of the transform Matrix?
    }
}
