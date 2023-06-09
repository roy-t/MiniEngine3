using System.Diagnostics;
using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine;

[Service]

internal sealed class RenderPipeline
{
    private readonly Device Device;
    private readonly FrameService FrameService;
    private readonly ImmediateDeviceContext ImmediateContext;
    private readonly MetricService MetricService;
    private readonly Queue<Task<CommandList>> GpuWorkQueue;
    private readonly Stopwatch Stopwatch;

    private readonly CameraSystem CameraSystem;
    private readonly PrimitiveSystem PrimitiveSystem;
    private readonly ImageBasedLightSystem ImageBasedLightSystem;
    private readonly SunLightSystem SunLightSystem;
    private readonly SkyboxSystem SkyboxSystem;
    private readonly LineSystem LineSystem;
    private readonly PostProcessingSystem PostProcessingSystem;

    public RenderPipeline(Device device, FrameService frameService, MetricService metricService,
        CameraSystem cameraSystem,
        PrimitiveSystem primitiveSystem,
        ImageBasedLightSystem imageBasedLightSystem,
        SunLightSystem sunLightSystem,
        SkyboxSystem skyboxSystem,
        LineSystem lineSystem,
        PostProcessingSystem postProcessingSystem
        )
    {
        this.Device = device;
        this.ImmediateContext = device.ImmediateContext;
        this.FrameService = frameService;
        this.MetricService = metricService;
        this.GpuWorkQueue = new Queue<Task<CommandList>>();
        this.Stopwatch = new Stopwatch();

        this.CameraSystem = cameraSystem;
        this.PrimitiveSystem = primitiveSystem;
        this.ImageBasedLightSystem = imageBasedLightSystem;
        this.SunLightSystem = sunLightSystem;
        this.SkyboxSystem = skyboxSystem;
        this.LineSystem = lineSystem;
        this.PostProcessingSystem = postProcessingSystem;
    }

    public void Run(Rectangle viewport, float alpha)
    {
        this.Stopwatch.Restart();

        this.RunInitializationStage();
        this.RunRenderStage(viewport, alpha);
        this.RunPostProcessStage();

        this.ProcessQueue();

        this.MetricService.Update("RenderPipeline.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }


    private void RunInitializationStage()
    {
        ClearBuffersSystem.Clear(this.Device.ImmediateContext, this.FrameService);
        this.CameraSystem.Update();
    }

    private void RunRenderStage(Rectangle viewport, float alpha)
    {
        this.Enqueue(this.PrimitiveSystem.Render(viewport, alpha));
        this.Enqueue(this.ImageBasedLightSystem.Render()); // TODO: system doesn't have output settings
        this.Enqueue(this.SunLightSystem.Render());  // TODO: system doesn't have output settings
        this.Enqueue(this.SkyboxSystem.Render());  // TODO: system doesn't have output settings
        this.Enqueue(this.LineSystem.Render(viewport, alpha));        
    }

    private void RunPostProcessStage()
    {
        this.Enqueue(this.PostProcessingSystem.Render()); // TODO: system doesn't have output settings
    }

    private void ProcessQueue()
    {
        while (this.GpuWorkQueue.Any())
        {
            var task = this.GpuWorkQueue.Dequeue();
            task.Wait();

            var commandList = task.Result;
            this.ImmediateContext.ExecuteCommandList(commandList);
            commandList.Dispose();
        }
    }


    private void Enqueue(Task<CommandList> task)
    {
        this.GpuWorkQueue.Enqueue(task);
    }
}
