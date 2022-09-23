namespace Mini.Engine.Core;
public static class RandomExtensions
{
    public static float InRange(this Random random, float min, float max)
    {
        var f = random.NextSingle();

        var range = max - min;

        return (f * range) + min;
    }
}
