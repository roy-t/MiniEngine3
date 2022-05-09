namespace Mini.Engine.UI;

internal sealed class MicroBenchmark
{
    private readonly string Name;
    private readonly float[] Samples;
    private float sum;
    private int index;
    private int seen;

    public MicroBenchmark(string name, int sampleLength = 60)
    {
        this.Name = name;
        this.Samples = new float[sampleLength];
    }

    public float AverageMs { get; private set; }

    public void Update(float elapsed)
    {        
        this.sum -= this.Samples[this.index];
        this.Samples[this.index] = elapsed * 1000.0f;
        this.sum += this.Samples[this.index];

        this.index = (this.index + 1) % this.Samples.Length;
        this.seen = Math.Min(this.Samples.Length, this.seen + 1);

        this.AverageMs = this.sum / this.seen;
    }

    public override string ToString()
    {
        return $"{this.Name} {this.AverageMs:F2}ms";
    }
}
