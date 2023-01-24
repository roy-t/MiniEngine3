using TileInstanceData = Mini.Engine.Content.Shaders.Generated.Tiles.InstanceData;

namespace Mini.Engine.Graphics.Tiles;
public static class TileBuilder
{
    public static TileInstanceData[] Create(int columns, int rows)
    {
        var tiles = new TileInstanceData[columns * rows];

        tiles[0] = new TileInstanceData() { Rotation = 1, Type = 1 };
        tiles[1] = new TileInstanceData() { Rotation = 2, Type = 2 };
        tiles[2] = new TileInstanceData() { Rotation = 2, Type = 1 };

        tiles[3] = new TileInstanceData() { Rotation = 1, Type = 2 };
        tiles[4] = new TileInstanceData() { Rotation = 0, Type = 0, Heigth = 1 };
        tiles[5] = new TileInstanceData() { Rotation = 3, Type = 2 };

        tiles[6] = new TileInstanceData() { Rotation = 0, Type = 1 };
        tiles[7] = new TileInstanceData() { Rotation = 0, Type = 2 };
        tiles[8] = new TileInstanceData() { Rotation = 3, Type = 3 };


        return tiles;
    }
}
