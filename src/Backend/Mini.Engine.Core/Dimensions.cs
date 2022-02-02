using System;

namespace Mini.Engine.Core;

public static class Dimensions
{
    public static int MipSlices(int resolution)
    {
        return 1 + (int)MathF.Floor(MathF.Log2(resolution));
    }
}
