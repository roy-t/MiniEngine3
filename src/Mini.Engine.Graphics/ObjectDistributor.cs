using System.Numerics;
using LibGame.Mathematics;
using Mini.Engine.Core;

namespace Mini.Engine.Graphics;

public sealed class ObjectDistributor
{
    private readonly WeightedRandom Random;

    private readonly int Length;
    private readonly int Stride;

    public ObjectDistributor(Vector2 min, Vector2 max, int seed, float[] weights, int stride)
    {
        this.Min = min;
        this.Max = max;

        var minWeight = weights.Min();
        var maxWeight = weights.Max();
        var positiveWeights = new float[weights.Length];
        for (var i = 0; i < weights.Length; i++)
        {
            positiveWeights[i] = Ranges.Map(weights[i], (minWeight, maxWeight), (0.0f, 1.0f));
            //if (positiveWeights[i] > 0.5f)
            //{
            //    positiveWeights[i] = 0.00f;
            //}
        }

        this.Random = new WeightedRandom(new Random(seed), positiveWeights);
        this.Length = weights.Length;
        this.Stride = stride;
    }

    public T[] Distribute<T>(int count, Func<Vector2, T> generator)
    {
        var items = new T[count];

        for (var i = 0; i < count; i++)
        {
            var index = this.Random.Next();

            var (x, y) = Indexes.ToTwoDimensional(index, this.Stride);
            var position = new Vector2(x, y);
            position = Ranges.Map(position, (0.0f, this.Stride), (this.Min.X, this.Max.X), (0.0f, this.Length / this.Stride), (this.Min.Y, this.Max.Y));

            items[i] = generator(position);
        }
        return items;
    }

    public Vector2 Min { get; }
    public Vector2 Max { get; }
}
