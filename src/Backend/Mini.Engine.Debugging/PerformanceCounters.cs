using System.Diagnostics;
using Mini.Engine.Configuration;

namespace Mini.Engine.Debugging;
[Service]
public sealed class PerformanceCounters
{
    // TODO: use perfmon.exe to figure out other useful measures! Like .Net used memroy for this instance

    private const string GPUProcessMemoryCategory = "GPU Process Memory";

    private readonly PerformanceCounter? GPUProcessMemoryCounter;

    public PerformanceCounters()
    {
        if (PerformanceCounterCategory.Exists(GPUProcessMemoryCategory))
        {
            var currentProcess = Environment.ProcessId;

            var category = new PerformanceCounterCategory(GPUProcessMemoryCategory);
            var instances = category.GetInstanceNames();
            var name = instances.Where(i => i.Contains(currentProcess.ToString())).FirstOrDefault();
            if (name != null)
            {

                this.GPUProcessMemoryCounter = new PerformanceCounter(GPUProcessMemoryCategory, "Dedicated Usage", name, true);
            }
        }
    }

    public float GetGPUMemoryUsageBytes()
    {
        return GPUProcessMemoryCounter?.NextValue() ?? 0;
    }
}
