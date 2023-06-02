using LibGame.Basics;
using Mini.Engine.Core;
using TileInstanceData = Mini.Engine.Content.Shaders.Generated.Tiles.InstanceData;

namespace Mini.Engine.Graphics.Tiles;
public static class TileBuilder
{
    public static TileInstanceData[] Create(int columns, int rows)
    {
        var tiles = new TileInstanceData[columns * rows];
        var random = Random.Shared;

        var bump = new TileInstanceData[9]
        {
            new TileInstanceData() { Rotation = 1, Type = 1 },
            new TileInstanceData() { Rotation = 2, Type = 2 },
            new TileInstanceData() { Rotation = 2, Type = 1 },
            new TileInstanceData() { Rotation = 1, Type = 2 },
            new TileInstanceData() { Rotation = 0, Type = 0, Heigth = 4 },
            new TileInstanceData() { Rotation = 3, Type = 2 },
            new TileInstanceData() { Rotation = 0, Type = 1 },
            new TileInstanceData() { Rotation = 0, Type = 2 },
            new TileInstanceData() { Rotation = 3, Type = 1 },
        };

        for (var c = 0; c < columns; c++)
        {
            for(var r = 0; r < rows; r++)
            {
                var i = Indexes.ToOneDimensional(c, r, columns);
                var ti = Indexes.ToOneDimensional(c % 3, r % 3, 3);

                tiles[i] = bump[ti];

                tiles[i].Heigth = (uint)random.Next(0, 4);
            }
        }


        return tiles;
    }
}
