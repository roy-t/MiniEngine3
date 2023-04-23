using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Diesel;

[Service]
internal class DieselUpdateLoop
{
    private readonly ComponentLifeCycleSystem LifeCycleSystem;


    private readonly MetricService MetricService;
    private readonly Stopwatch Stopwatch;

    public DieselUpdateLoop(ComponentLifeCycleSystem lifeCycleSystem, MetricService metricService)
    {
        this.LifeCycleSystem = lifeCycleSystem;

        this.MetricService = metricService;

        this.Stopwatch = new Stopwatch();
    }

    public void Run(float elapsed)
    {
        this.Stopwatch.Restart();

        this.LifeCycleSystem.Process();

        this.MetricService.Update("DieselUpdateLoop.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }
}
