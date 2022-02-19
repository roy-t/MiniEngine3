using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Models;

namespace Mini.Engine;

[Service]
internal sealed class DebugPipelineBuilder
{
    private readonly PipelineBuilder Builder;

    public DebugPipelineBuilder(PipelineBuilder builder)
    {
        this.Builder = builder;
    }

    public ParallelPipeline Build()
    {
        var pipeline = this.Builder.Builder();
        return pipeline
            .System<ClearDebugBuffersSystem>()
                .InSequence()
                .Produces("Initialization", "DebugBuffer")
                .Build()
            .System<BoundsSystem>()
                .InSequence()
                .Requires("Initialization", "DebugBuffer")
                .Produces("Debug", "Outlines")
                .Build()
        .Build();
    }
}
