using System.Numerics;
using Mini.Engine.Core;
using HexagonInstanceData = Mini.Engine.Content.Shaders.Generated.Hexagon.InstanceData;

namespace Mini.Engine.Graphics.Hexagons;
public static class HexagonTerrainBuilder
{

    public static HexagonInstanceData[] Create(int columns, int rows)
    {
        var width = 0.5f * MathF.Sin((MathF.PI * 2) / 6);
        var heigth = (1.0f + 0.5f) * 0.5f;

        var data = new HexagonInstanceData[rows * columns];

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < columns; c++)
            {
                var offset = r % 2 == 0
                    ? new Vector3(width, 0, 0)
                    : Vector3.Zero;

                var index = Indexes.ToOneDimensional(c, r, columns);
                data[index] = new HexagonInstanceData()
                {
                    Position = new Vector3(width * c * 2, 0, r * heigth) + offset,
                    Sides = 0
                };
            }
        }

        return data;
    }
}
