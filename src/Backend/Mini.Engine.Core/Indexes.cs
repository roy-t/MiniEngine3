namespace Mini.Engine.Core;

public static class Indexes
{
    public static int ToOneDimensional(int x, int y, int stride)
    {
        return x + (stride * y);
    }

    public static (int, int) ToTwoDimensional(int i, int stride)
    {
        var x = i % stride;
        var y = i / stride;

        return (x, y);
    }
}
