using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Core;
using HexagonInstanceData = Mini.Engine.Content.Shaders.Generated.Hexagon.InstanceData;

namespace Mini.Engine.Graphics.Hexagons;
public static class HexagonTerrainBuilder
{
    public static HexagonInstanceData[] Create(int columns, int rows)
    {
        var stepY = 0.05f * 2.0f;

        var stepX = 0.5f * MathF.Sin((MathF.PI * 2) / 6);
        var stepZ = (1.0f + 0.5f) * 0.5f;

        var data = new HexagonInstanceData[rows * columns];

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < columns; c++)
            {
                
                var offset = r % 2 == 0
                    ? new Vector3(stepX, 0, 0)
                    : Vector3.Zero;


                var y = stepY * r;

                var index = Indexes.ToOneDimensional(c, r, columns);
                var arr0 = new float[16] { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
                var arr1 = new float[16] { 1, 1, /* filler */ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                var s0 = PackSides(arr0);
                var s1 = PackSides(arr1);

                data[index] = new HexagonInstanceData()
                {
                    Position = new Vector3(stepX * c * 2, y, r * stepZ) + offset,
                    S0 = s0,
                    S1 = s1
                };
            }
        }

        //ComputeOffsets(data, columns, rows);

        return data;
    }

    private static uint PackSides(float[] offsets)
    {
        Debug.Assert(offsets.Length == 16);

        var states = offsets.Select(o => (Tristate)(Math.Sign(o) + 1)).ToArray();

        return BitPacker.Pack(states);
    }



    //private static void ComputeOffsets(HexagonInstanceData[] data, int columns, int rows)
    //{
    //    for (var r = 0; r < rows; r++)
    //    {
    //        for (var c = 0; c < columns; c++)
    //        {
    //            var index = Indexes.ToOneDimensional(c, r, columns);

    //            var n = (c, r - 2);
    //            var ne = (c, r - 1);
    //            var se = (c, r + 1);
    //            var s = (c, r + 2);
    //            var sw = (c - 1, r + 1);
    //            var nw = (c - 1, r - 1);

    //            data[index].Ne = GetSlope(data, index, columns, rows, index, n, ne);
    //            data[index].E = GetSlope(data, index, columns, rows, index, ne, se);
    //            data[index].Se = GetSlope(data, index, columns, rows, index, se, s);
    //            data[index].Sw = GetSlope(data, index, columns, rows, index, s, sw);
    //            data[index].W = GetSlope(data, index, columns, rows, index, sw, nw);
    //            data[index].Nw = GetSlope(data, index, columns, rows, index, nw, n);
    //        }
    //    }
    //}

    //private static int GetSlope(HexagonInstanceData[] data, int hexagon, int columns, int rows, int index, (int column, int rows) ccw, (int column, int row) cw)
    //{
    //    var ccwSlope = GetSlope(data, hexagon, columns, rows, index, ccw);
    //    var cwSlope = GetSlope(data, hexagon, columns, rows, index, cw);

    //    // if both sides are opposing or any side is 0, return 0
    //    if (ccwSlope + cwSlope == 0 || ccwSlope == 0 || cwSlope == 0)
    //    {
    //        return 0;
    //    }

    //    return ccwSlope; // both values are the same
    //}

    //private static int GetSlope(HexagonInstanceData[] data, int hexagon, int columns, int rows, int index, (int column, int rows) neighbour)
    //{
    //    if (neighbour.column >= 0 && neighbour.column < columns && neighbour.rows >= 0 && neighbour.rows < rows)
    //    {
    //        var height = data[hexagon].Position.Y;
    //        var targetHeight = data[Indexes.ToOneDimensional(neighbour.column, neighbour.rows, columns)].Position.Y;

    //        return Math.Sign(targetHeight - height);
    //    }

    //    return 0;
    //}
}
