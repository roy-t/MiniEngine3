﻿using System.Diagnostics;
using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine;

[Service]
internal sealed record class RenderSystems
(    
    PrimitiveSystem Primitive,
    ModelSystem Model,
    ImageBasedLightSystem ImageBasedLight,
    SunLightSystem SunLight,
    CascadedShadowMapSystem CascadedShadowMap,
    PointLightSystem PointLight,
    SkyboxSystem Skybox,
    LineSystem Line,
    PostProcessingSystem PostProcessing
);

[Service]
internal sealed class RenderPipeline
{
    private readonly ImmediateDeviceContext ImmediateContext;
    private readonly MetricService MetricService;
    private readonly Queue<Task<CommandList>> GpuWorkQueue;
    private readonly Stopwatch Stopwatch;

    private readonly RenderSystems Systems;

    public RenderPipeline(Device device, MetricService metricService, RenderSystems systems)
    {
        this.ImmediateContext = device.ImmediateContext;
        this.MetricService = metricService;
        this.GpuWorkQueue = new Queue<Task<CommandList>>();
        this.Stopwatch = new Stopwatch();

        this.Systems = systems;
    }

    public void Run(in Rectangle viewport, in Rectangle scissor, float alpha)
    {
        this.Stopwatch.Restart();

        // We create draw commands in parallel and then draw in sequence.
        // But even then we need to be careful as some systems base their draw commands
        // on outputs of other systems. Ideally this isn't the case and these updates
        // happen only in the update loop, while the draw loop is read only.
        // But for now, this isnt' the case, especially with the
        // CascadedShadowMap and the Sunlight systems. So we run in separate stages
        // that complete their draw before the next stage starts

        this.RunGeometryStage(in viewport, in scissor, alpha);
        this.RunLightStage(in viewport, in scissor, alpha);        
        this.RunPostProcessStage(in viewport, in scissor);

        this.MetricService.Update("RenderPipeline.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }

    private void RunGeometryStage(in Rectangle viewport, in Rectangle scissor, float alpha)
    {
        this.Enqueue(this.Systems.Primitive.Render(viewport, scissor, alpha));
        this.Enqueue(this.Systems.Model.Render(viewport, scissor, alpha));

        this.Enqueue(this.Systems.CascadedShadowMap.Render(alpha));
        //this.ProcessQueue();
    }

    private void RunLightStage(in Rectangle viewport, in Rectangle scissor, float alpha)
    {
        this.Enqueue(this.Systems.PointLight.Render(viewport, scissor, alpha));
        this.Enqueue(this.Systems.ImageBasedLight.Render(viewport, scissor));
        this.Enqueue(this.Systems.SunLight.Render(viewport, scissor));
        this.Enqueue(this.Systems.Skybox.Render(viewport, scissor));
        this.Enqueue(this.Systems.Line.Render(viewport, scissor, alpha));

        //this.ProcessQueue();
    }

    private void RunPostProcessStage(in Rectangle viewport, in Rectangle scissor)
    {
        this.Enqueue(this.Systems.PostProcessing.Render(viewport, scissor));

        this.ProcessQueue();
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
