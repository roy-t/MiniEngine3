using System.Diagnostics;
using Mini.Engine.Configuration;

namespace Mini.Engine;

public struct Gauge
{
    public string Tag;
    public string Unit;
    public int Index;
    public float[] Values;
}

[Service]
public sealed class MetricService
{
    private const int Memory = 50;

    private Gauge[] gauges;
    public int count;

    public MetricService()
    {
        this.gauges = new Gauge[10];
    }

    [Conditional("DEBUG")]
    public void Update(string tag, float value)
    {
        for (var i = 0; i < this.gauges.Length; i++)
        {
            ref var gauge = ref this.gauges[i];
            if (gauge.Tag == tag)
            {
                this.Update(ref gauge, tag, value);
                return;
            }
        }

        if (this.count >= gauges.Length)
        {
            Array.Resize(ref this.gauges, this.gauges.Length * 2);
        }

        this.Update(ref this.gauges[this.count], tag, value);
        this.count++;
    }

    public ReadOnlySpan<Gauge> Gauges => new(this.gauges, 0, this.count);

    private void Update(ref Gauge gauge, string tag, float value)
    {
        if (gauge.Values == null)
        {
            gauge.Tag = tag;
            gauge.Values = new float[Memory];
        }

        gauge.Values[gauge.Index] = value;
        gauge.Index = (gauge.Index + 1) % Memory;
    }

    public (float average, float min, float max) Statistics(in Gauge gauge)
    {
        var min = float.MaxValue;
        var max = float.MinValue;
        var average = 0.0f;

        for (var i = 0; i < gauge.Values.Length; i++)
        {
            var value = gauge.Values[i];
            min = Math.Min(value, min);
            max = Math.Max(value, max);
            average += value;
        }

        average /= Memory;

        return (average, min, max);
    }

    public static string GuessUnit(string tag)
    {
        if (tag.EndsWith("millis", StringComparison.OrdinalIgnoreCase))
        {
            return "ms";
        }

        return string.Empty;
    }
}
