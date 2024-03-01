using System.Numerics;
using Mini.Engine.Titan.Terrains;
using Vortice.Mathematics;
using Xunit;
using static Xunit.Assert;
namespace Mini.Engine.Tests;
public class TerrainBVHTests
{
    private static IReadOnlyGrid<Tile> CreateTiles()
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
        return new Grid<Tile>(tiles, 4, 4);
    }

    [Fact]
    public void BVH()
    {
        var tiles = CreateTiles();
        var bvh = new TerrainBVH(tiles);

        //Equal(10, bvh.GetHeight(0, 0, 4));
        //Equal(9, bvh.GetHeight(1, 0, 4));
        //Equal(2, bvh.GetHeight(2, 2, 4));
        //Equal(11, bvh.GetHeight(3, 3, 4));

        //Equal(20, bvh.GetHeight(0, 0, 2));
        //Equal(18, bvh.GetHeight(1, 0, 2));
        //Equal(5, bvh.GetHeight(0, 1, 2));
        //Equal(11, bvh.GetHeight(1, 1, 2));

        //Equal(20, bvh.GetHeight(0, 0, 1));

        //var expectedFull = new BoundingBox(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(4.0f, 20.0f, 4.0f));
        //var actualFull = bvh.GetBounds(0, 0, 1);
        //Equal(expectedFull, actualFull);

        //var expectedQuarter = new BoundingBox(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(2.0f, 20.0f, 2.0f));
        //var actualQuarter = bvh.GetBounds(0, 0, 2);
        //Equal(expectedQuarter, actualQuarter);

        //var expectedSingle = new BoundingBox(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 10.0f, 1.0f));
        //var actualSingle = bvh.GetBounds(0, 0, 4);
        //Equal(expectedSingle, actualSingle);

        var ray = new Ray(new Vector3(2.5f, 1000.0f, 1.5f), new Vector3(0.0f, -1.0f, 0.0f));
        var hit = bvh.CheckTileHit(ray, out var index, out var position);

        True(hit);
        Equal(6, index);
        Equal(new Vector3(2.5f, 18.0f, 1.5f), position);
    }
}
