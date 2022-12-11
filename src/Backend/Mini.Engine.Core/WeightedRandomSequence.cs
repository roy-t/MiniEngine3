namespace Mini.Engine.Core;
internal class WeightedRandomSequence<T>
{
    private record Option(T Value, float Ratio);

    private readonly List<Option> Options;
    private readonly Random Random;

    private float ratioSum;

    public WeightedRandomSequence(int seed)
    {
        this.Options = new List<Option>();
        this.Random = new Random(seed);
    }

    public void AddOption(T value, float ratio)
    {
        this.ratioSum += ratio;
        this.Options.Add(new Option(value, ratio));
    }

    public T Next()
    {
        var roll = this.Random.NextSingle() * this.ratioSum;

        for (var i = 0; i < this.Options.Count; i++)
        {
            var option = this.Options[i];
            roll -= option.Ratio;
            if (roll <= 0.0f)
            {
                return option.Value;
            }
        }

        throw new Exception("Next called without any elements in this weighted random sequence");
    }
}
