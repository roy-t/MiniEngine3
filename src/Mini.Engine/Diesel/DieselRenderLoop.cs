using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Diesel;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel;

[Service]
internal class DieselRenderLoop
{
    private readonly Device Device;
    private readonly ImmediateDeviceContext ImmediateContext;

    private readonly MetricService MetricService;

    private readonly PrimitiveSystem PrimitiveSystem;

    private readonly Queue<Task<CommandList>> GpuWorkQueue;

    public DieselRenderLoop(Device device, PrimitiveSystem primitiveSystem, MetricService metricService)
    {
        this.Device = device;
        this.PrimitiveSystem = primitiveSystem;
        this.ImmediateContext = device.ImmediateContext;

        this.GpuWorkQueue = new Queue<Task<CommandList>>();
        this.MetricService = metricService;
    }

    public void Run(RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int heigth, float alpha)
    {
        var stopwatch = Stopwatch.StartNew();

        ClearBuffersSystem.Clear(this.Device, albedo, Colors.Transparent);
        ClearBuffersSystem.Clear(this.Device, depth);        

        this.Enqueue(this.PrimitiveSystem.DrawPrimitives(albedo, depth, x, y, width, heigth, alpha));

        while (this.GpuWorkQueue.Any())
        {
            var task = this.GpuWorkQueue.Dequeue();
            task.Wait();            

            var commandList = task.Result;
            this.ImmediateContext.ExecuteCommandList(commandList);
            commandList.Dispose();
        }

        this.MetricService.Update("DieselRenderLoop.Run.Millis", (float)stopwatch.Elapsed.TotalMilliseconds);
    }

    private void Enqueue(Task<CommandList> task)
    {
        this.GpuWorkQueue.Enqueue(task);
    }
}
