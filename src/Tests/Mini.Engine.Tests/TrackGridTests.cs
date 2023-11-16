using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.Modelling.Curves;
using Xunit;

namespace Mini.Engine.Tests;
public static class TrackGridTests
{
    [Fact]
    public static void SmokeTest()
    {
        var grid = new TrackGrid(9, 9, 1, 1);
        
        var cell = grid.PickCell(new Vector3(0, 0, 0));
        Assert.Equal(4, cell.x);
        Assert.Equal(4, cell.y);

        cell = grid.PickCell(new Vector3(0, 0, 0.49f));
        Assert.Equal(4, cell.x);
        Assert.Equal(4, cell.y);

        cell = grid.PickCell(new Vector3(0, 0, -0.49f));
        Assert.Equal(4, cell.x);
        Assert.Equal(4, cell.y);

        var straight = new StraightCurve(new Vector3(0, 0, 0.5f), new Vector3(0, 0, -1), 1.0f);
        grid.Add(cell.x, cell.y, straight, Transform.Identity);

        var placement = grid[cell.x, cell.y].Placements;
        Assert.Equal(2, placement.Count);

        Assert.Equal(straight, placement[0].Curve);
        Assert.Equal(Transform.Identity, placement[0].Transform);

        var transformA = Transform.Identity;
        var transformB = new Transform().SetTranslation(new Vector3(0, 0, -1));
        var connected = straight.IsConnectedTo(1.0f, transformA, straight, 0.0f, transformB, 0.1f);
        Assert.True(connected);

        // Note the cell.y -1 here
        grid.Add(cell.x, cell.y - 1, straight, transformB);

        var connections = new List<ConnectedToReference>();
        grid.GetConnections(cell.x, cell.y, 0, 1.0f, connections);

        Assert.Single(connections);        
        Assert.Equal(new ConnectedToReference(cell.x, cell.y, 0, 1.0f, cell.x, cell.y - 1, 0, 0.0f), connections[0]);

        grid.Remove(cell.x, cell.y, 1);
        grid.Remove(cell.x, cell.y, 0);
        placement = grid[cell.x, cell.y].Placements;
        Assert.Empty(placement);        
    }
}
