using Mini.Engine.Titan.Terrains;
using Xunit;
using static Xunit.Assert;
namespace Mini.Engine.Tests;
public class TerrainBVHTests
{

    [Fact]
    public void BVH()
    {
        var tiles = new Tile[]
        {
            new Tile(10),
            new Tile(9),
            new Tile(8),
            new Tile(7),

            new Tile(20),
            new Tile(19),
            new Tile(18),
            new Tile(17),

            new Tile(0),
            new Tile(1),
            new Tile(2),
            new Tile(3),

            new Tile(4),
            new Tile(5),
            new Tile(6),
            new Tile(11),
        };

        var bvh = new TerrainBVH(tiles, 4, 4);

        Equal(10, bvh.GetHeight(0, 0, 4));
        Equal(9, bvh.GetHeight(1, 0, 4));
        Equal(2, bvh.GetHeight(2, 2, 4));
        Equal(11, bvh.GetHeight(3, 3, 4));

        Equal(20, bvh.GetHeight(0, 0, 2));
        Equal(18, bvh.GetHeight(1, 0, 2));
        Equal(5, bvh.GetHeight(0, 1, 2));
        Equal(11, bvh.GetHeight(1, 1, 2));

        Equal(20, bvh.GetHeight(0, 0, 1));


        var foo = bvh.GetBounds(0, 0, 2);

        Equal(??, foo);
    }
}
