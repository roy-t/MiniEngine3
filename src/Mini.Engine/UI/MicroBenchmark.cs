using System;

namespace Mini.Engine.UI;

internal sealed class MicroBenchmark
{
    private readonly string Name;
    private readonly float[] Samples;
    private int index;
    private int seen;

    public MicroBenchmark(string name, int sampleLength = 300)
    {
        this.Name = name;
        this.Samples = new float[sampleLength];
    }

    public float AverageMs { get; private set; }

    public void Update(float elapsed)
    {
        this.Samples[this.index] = elapsed;
        this.index = (this.index + 1) % this.Samples.Length;
        this.seen = Math.Max(this.seen, this.index - 1);

        var aggregate = 0.0f;
        for (var i = 0; i < this.seen; i++)
        {
            aggregate += this.Samples[i];
        }

        if (this.seen > 0)
        {
            this.AverageMs = (aggregate / this.seen) * 1000.0f;
        }
    }

    public override string ToString()
    {
        return $"{this.Name} {this.AverageMs:F2}ms";
    }
}
