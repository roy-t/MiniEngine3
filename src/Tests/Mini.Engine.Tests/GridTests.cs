using System.Numerics;
using Mini.Engine.Core;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;

public class GridTests
{
    [Fact]
    public void SmokeTest()
    {
        var grid = new Grid<Vector2>(0, 10, 0, 10, 2, 2);
        grid.Fill((x, y, c, r) => new Vector2(x, y));

        var value = grid.Get(3, 3);
        Equal(new Vector2(0, 0), value);
    }
}
