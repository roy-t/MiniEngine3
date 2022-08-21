using System.Diagnostics;
using Mini.Engine.Configuration;

namespace Mini.Engine.Debugging;
[Service]
public sealed class PerformanceCounters
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
  
    public class PerformanceAggregator
    {
        private PerformanceCounter? counter;        
        private float lastValue;
        private int ticks;

        public PerformanceAggregator(string categoryName, string counterName, string? instanceFilter = null, float scale = 1.0f)
        {
            this.Scale = scale;

            Task.Run(() =>
            {
                if (PerformanceCounterCategory.CounterExists(counterName, categoryName))
                {
                    var category = new PerformanceCounterCategory(categoryName);
                    if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                    {
                        var processName = Process.GetCurrentProcess().ProcessName;
                        var processId = Environment.ProcessId.ToString();

                        IEnumerable<string> names = category.GetInstanceNames();
                        if(!string.IsNullOrEmpty(instanceFilter))
                        {
                            names = names.Where(n => n.Contains(instanceFilter));
                        }

                        var instanceName = names
                            .Where(n => n.Contains(processName) || n.Contains(processId))
                            .SingleOrDefault();

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
            });
        }

        public float Scale { get; }

        public float Value
        {
            get {
                if (this.ticks++ % 100 == 0)
                {
                    this.lastValue = (this.counter?.NextValue() ?? 0.0f) * this.Scale;
                }

                return this.lastValue;
            }            
        }
    }

}
