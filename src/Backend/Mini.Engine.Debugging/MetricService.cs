using System.Diagnostics;
using Mini.Engine.Configuration;

namespace Mini.Engine.Debugging;

public struct Gauge
{
    public string Tag;

    public float Min;
    public float Max;
    public float Average;

    internal float NextMin;
    internal float NextMax;
    internal float Total;
    internal float Measurements;

    internal DateTime LastSwap;
}

[Service]
public sealed class MetricService : IDisposable
{
    private const string HostGpuMemoryUsage = "Host.GPU.MemoryUsage.MiB";
    private const string HostGpuUsage = "Host.GPU.Usage.%";
    private const string HostCpuMemoryUsage = "Host.CPU.MemoryUsage.MiB";
    private const string HostCpuUsage = "Host.CPU.Usage.%";

    private readonly PerformanceCounters Counters;
    private Gauge[] gauges;
    public int count;

    public MetricService()
    {
        this.Counters = new PerformanceCounters();
        this.gauges = new Gauge[4];

        this.UpdateBuiltInGauges();
    }

    [Conditional("DEBUG")]
    public void Update(string tag, float value)
    {
        for (var i = 0; i < this.gauges.Length; i++)
        {
            ref var gauge = ref this.gauges[i];
            if (gauge.Tag == tag)
            {
                Update(ref gauge, tag, value);
                return;
            }
        }

        if (this.count >= this.gauges.Length)
        {
            Array.Resize(ref this.gauges, this.gauges.Length * 2);
        }

        Update(ref this.gauges[this.count], tag, value);
        this.count++;
    }

    public void UpdateBuiltInGauges()
    {
        this.Update(HostGpuMemoryUsage, this.Counters.GPUMemoryCounter.Value / (1024 * 1024));
        this.Update(HostGpuUsage, this.Counters.GPUUsageCounter.Value);
        this.Update(HostCpuMemoryUsage, this.Counters.CPUMemoryCounter.Value / (1024 * 1024));
        this.Update(HostCpuUsage, this.Counters.CPUUsageCounter.Value);
    }

    public ReadOnlySpan<Gauge> Gauges => new(this.gauges, 0, this.count);

    private static void Update(ref Gauge gauge, string tag, float value)
    {
        gauge.Tag = tag;

        gauge.NextMin = Math.Min(gauge.NextMin, value);
        gauge.NextMax = Math.Max(gauge.NextMax, value);
        gauge.Total += value;

        gauge.Measurements += 1;

        if ((DateTime.Now - gauge.LastSwap) > TimeSpan.FromSeconds(0.25))
        {
            gauge.Min = gauge.NextMin;
            gauge.Max = gauge.NextMax;
            gauge.Average = gauge.Total / gauge.Measurements;

            gauge.NextMin = float.MaxValue;
            gauge.NextMax = float.MinValue;
            gauge.Total = 0;
            gauge.Measurements = 0;
            gauge.LastSwap = DateTime.Now;
        }
    }

    public void Dispose()
    {
        this.Counters.Dispose();
    }
}
