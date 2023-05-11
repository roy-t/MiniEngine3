using System.Diagnostics;

namespace Mini.Engine.Debugging;

internal sealed class PerformanceCounters : IDisposable
{
    public readonly PerformanceAggregator GPUMemoryCounter;
    public readonly PerformanceAggregator GPUUsageCounter;

    public readonly PerformanceAggregator CPUMemoryCounter;
    public readonly PerformanceAggregator CPUUsageCounter;

    private readonly Thread MonitorThread;
    private bool isRunning;

    public PerformanceCounters()
    {
        this.GPUMemoryCounter = new PerformanceAggregator("GPU Process Memory", "Dedicated Usage");
        this.GPUUsageCounter = new PerformanceAggregator("GPU Engine", "Utilization Percentage", instanceFilter: "engtype_3D");
        this.CPUMemoryCounter = new PerformanceAggregator("Process", "Working Set - Private");
        this.CPUUsageCounter = new PerformanceAggregator("Process", "% Processor Time", scale: 1.0f / Environment.ProcessorCount);

        this.MonitorThread = new Thread(() =>
        {
            var counters = new PerformanceAggregator[] { this.GPUMemoryCounter, this.GPUUsageCounter, this.CPUMemoryCounter, this.CPUUsageCounter };

            foreach (var counter in counters)
            {
                counter.InitializeCounter();
            }

            while (this.isRunning)
            {
                foreach (var counter in counters)
                {
                    if (this.isRunning)
                    {
                        counter.Update();
                    }
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(20));
            }
        });

        this.isRunning = true;
        this.MonitorThread.IsBackground = true;
        this.MonitorThread.Priority = ThreadPriority.Lowest;
        this.MonitorThread.Start();
    }

    public void Dispose()
    {
        this.isRunning = false;
        this.MonitorThread.Join();

        this.GPUMemoryCounter.Dispose();
        this.GPUUsageCounter.Dispose();

        this.CPUMemoryCounter.Dispose();
        this.CPUUsageCounter.Dispose();
    }

    public sealed class PerformanceAggregator : IDisposable
    {
        private PerformanceCounter? counter;

        public PerformanceAggregator(string categoryName, string counterName, string? instanceFilter = null, float scale = 1.0f)
        {
            this.CategoryName = categoryName;
            this.CounterName = counterName;
            this.InstanceFilter = instanceFilter ?? string.Empty;
            this.Scale = scale;
        }
        public float Value { get; private set; }

        public string CategoryName { get; }
        public string CounterName { get; }
        public string InstanceFilter { get; }
        public float Scale { get; }

        internal void Update()
        {
            this.Value = (this.counter?.NextValue() ?? 0.0f) * this.Scale;
        }

        internal void InitializeCounter()
        {
            if (PerformanceCounterCategory.CounterExists(this.CounterName, this.CategoryName))
            {
                var category = new PerformanceCounterCategory(this.CategoryName);
                if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                {
                    var processName = Process.GetCurrentProcess().ProcessName;
                    var processId = Environment.ProcessId.ToString();

                    IEnumerable<string> names = category.GetInstanceNames();
                    if (!string.IsNullOrEmpty(this.InstanceFilter))
                    {
                        names = names.Where(n => n.Contains(this.InstanceFilter));
                    }

                    var instanceName = names
                        .Where(n => n.Contains(processName) || n.Contains(processId))
                        .FirstOrDefault();

                    if (instanceName != null)
                    {
                        this.counter = new PerformanceCounter(this.CategoryName, this.CounterName, instanceName, true);
                    }
                }
                else
                {
                    this.counter = new PerformanceCounter(this.CategoryName, this.CounterName, true);
                }
            }
        }

        public void Dispose()
        {
            this.counter?.Dispose();
            this.counter = null;
        }
    }
}
