using System.Diagnostics;
using Mini.Engine.Configuration;

namespace Mini.Engine.Debugging;
[Service]
public sealed class PerformanceCounters
{
    // TODO: use perfmon.exe to figure out other useful measures! Like .Net used memroy for this instance

    private const string GPUProcessMemoryCategory = "GPU Process Memory";

    private PerformanceCounter? gpuProcessMemoryCounter;

    public PerformanceCounters()
    {
        var task = Task.Run(() =>
        {
            if (PerformanceCounterCategory.Exists(GPUProcessMemoryCategory))
            {
                var currentProcess = Environment.ProcessId;

                var category = new PerformanceCounterCategory(GPUProcessMemoryCategory);
                var instances = category.GetInstanceNames();
                var name = instances.Where(i => i.Contains(currentProcess.ToString())).FirstOrDefault();
                if (name != null)
                {

                    this.gpuProcessMemoryCounter = new PerformanceCounter(GPUProcessMemoryCategory, "Dedicated Usage", name, true);
                }
            }
        });
    }

    public float GetGPUMemoryUsageBytes()
    {
        return this.gpuProcessMemoryCounter?.NextValue() ?? 0;
    }
}
