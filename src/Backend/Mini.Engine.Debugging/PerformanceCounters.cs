using System.Diagnostics;

namespace Mini.Engine.Debugging;

internal sealed class PerformanceCounters : IDisposable
{
    public readonly PerformanceAggregator GPUMemoryCounter;
    public readonly PerformanceAggregator GPUUsageCounter;

    public readonly PerformanceAggregator CPUMemoryCounter;
    public readonly PerformanceAggregator CPUUsageCounter;

    public PerformanceCounters()
    {
        this.GPUMemoryCounter = new PerformanceAggregator("GPU Process Memory", "Dedicated Usage");
        this.GPUUsageCounter = new PerformanceAggregator("GPU Engine", "Utilization Percentage", instanceFilter: "engtype_3D");
        this.CPUMemoryCounter = new PerformanceAggregator("Process", "Working Set - Private");
        this.CPUUsageCounter = new PerformanceAggregator("Process", "% Processor Time", scale: 1.0f / Environment.ProcessorCount);
    }

    public void Dispose()
    {
        this.GPUMemoryCounter.Dispose();
        this.GPUUsageCounter.Dispose();

        this.CPUMemoryCounter.Dispose();
        this.CPUUsageCounter.Dispose();
    }

    public sealed class PerformanceAggregator : IDisposable
    {
        private bool initializedCounter;
        private PerformanceCounter? counter;
        private Timer? timer;

        private readonly float Scale;

        public PerformanceAggregator(string categoryName, string counterName, string? instanceFilter = null, float scale = 1.0f)
        {
            this.Scale = scale;
            this.timer = new Timer(_ =>
            {
                if (!this.initializedCounter)
                {                    
                    this.InitializeCounter(categoryName, counterName, instanceFilter);
                    this.initializedCounter = true;
                }

                if (this.counter == null)
                {
                    this.Dispose();
                }
                else
                {
                    this.Value = (this.counter?.NextValue() ?? 0.0f) * this.Scale;
                }
            }, null, TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20));
        }

        private void InitializeCounter(string categoryName, string counterName, string? instanceFilter)
        {
            if (PerformanceCounterCategory.CounterExists(counterName, categoryName))
            {
                var category = new PerformanceCounterCategory(categoryName);
                if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                {
                    var processName = Process.GetCurrentProcess().ProcessName;
                    var processId = Environment.ProcessId.ToString();

                    IEnumerable<string> names = category.GetInstanceNames();
                    if (!string.IsNullOrEmpty(instanceFilter))
                    {
                        names = names.Where(n => n.Contains(instanceFilter));
                    }

                    var instanceName = names
                        .Where(n => n.Contains(processName) || n.Contains(processId))
                        .FirstOrDefault();

                    if (instanceName != null)
                    {
                        this.counter = new PerformanceCounter(categoryName, counterName, instanceName, true);
                    }
                }
                else
                {
                    this.counter = new PerformanceCounter(categoryName, counterName, true);
                }
            }
        }

        public float Value { get; private set; }

        public void Dispose()
        {
            this.timer?.Dispose();
            this.timer = null;

            this.counter = null;
            this.counter?.Dispose();
        }
    }
}
