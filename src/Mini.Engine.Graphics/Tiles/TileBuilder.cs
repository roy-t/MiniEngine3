using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TileInstanceData = Mini.Engine.Content.Shaders.Generated.Tiles.InstanceData;

namespace Mini.Engine.Graphics.Tiles;
public static class TileBuilder
{
    public static TileInstanceData[] Create(int columns, int rows)
    {
        var tiles = new TileInstanceData[columns * rows];

        return tiles;
    }
}
