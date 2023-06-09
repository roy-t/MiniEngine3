using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Diesel;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel;

[Service]
internal class DieselRenderLoop
{
    private readonly Device Device;
    private readonly ImmediateDeviceContext ImmediateContext;
    private readonly MetricService MetricService;

    private readonly Queue<Task<CommandList>> GpuWorkQueue;
    private readonly Stopwatch Stopwatch;

    private readonly CameraService CameraService;
    private readonly CameraController CameraController;
    private readonly PrimitiveSystem PrimitiveSystem;
    private readonly LineSystem LineSystem;

    public DieselRenderLoop(Device device, MetricService metricService, PrimitiveSystem primitiveSystem, LineSystem lineSystem, CameraController cameraController, CameraService cameraService)
    {
        this.Device = device;
        this.ImmediateContext = device.ImmediateContext;
        this.MetricService = metricService;

        this.GpuWorkQueue = new Queue<Task<CommandList>>();
        this.Stopwatch = new Stopwatch();

        this.CameraController = cameraController;
        this.CameraService = cameraService;
        this.PrimitiveSystem = primitiveSystem;
        this.LineSystem = lineSystem;
    }

    public void Run(RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int heigth, float alpha, float elapsedRealWorldTime)
    {
        this.Stopwatch.Restart();

        ClearBuffersSystem.Clear(this.Device, albedo, Colors.Transparent);
        ClearBuffersSystem.Clear(this.Device, depth);

        ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();
        this.CameraController.Update(elapsedRealWorldTime, ref cameraTransform.Current);

        //this.Enqueue(this.PrimitiveSystem.DrawPrimitives(albedo, depth, x, y, width, heigth, alpha));
        this.Enqueue(this.LineSystem.DrawLines(albedo, depth, x, y, width, heigth, alpha));

        while (this.GpuWorkQueue.Any())
        {
            var task = this.GpuWorkQueue.Dequeue();
            task.Wait();

            var commandList = task.Result;
            this.ImmediateContext.ExecuteCommandList(commandList);
            commandList.Dispose();
        }

        this.MetricService.Update("DieselRenderLoop.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }

    private void Enqueue(Task<CommandList> task)
    {
        this.GpuWorkQueue.Enqueue(task);
    }
}
