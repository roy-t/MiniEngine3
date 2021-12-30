using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Models;

namespace Mini.Engine;

[Service]
internal sealed class RenderPipelineBuilder
{
    private readonly PipelineBuilder Builder;

    public RenderPipelineBuilder(PipelineBuilder builder)
    {
        this.Builder = builder;
    }

    public ParallelPipeline Build()
    {
        var pipeline = this.Builder.Builder();
        return pipeline
            .System<ComponentFlushSystem>()
                .Parallel()
                .Produces("Initialization", "Containers")
                .Build()
            .System<ClearGBufferSystem>()
                .InSequence()
                .Produces("Initialization", "GBuffer")
                .Build()
            .System<ModelSystem>()
                .InSequence()
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Models")
                .Build()
        .Build();
    }
}
