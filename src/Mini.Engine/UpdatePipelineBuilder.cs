using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.Vegetation;

namespace Mini.Engine;

[Service]
internal sealed class UpdatePipelineBuilder
{
    private readonly PipelineBuilder Builder;

    public UpdatePipelineBuilder(PipelineBuilder builder)
    {
        this.Builder = builder;
    }

    public ParallelPipeline Build()
    {
        var pipeline = this.Builder.Builder();
        return pipeline
           .System<WindSystem>()
               .Parallel()
               .Produces("World", "Wind")
               .Build()
           .System<ComponentLifeCycleSystem>()
               .Parallel()
               .Produces("Initialization", "Containers")
               .Build()
           .System<TransformSystem>()
               .Parallel()
               .Produces("Initialization", "Transforms")
               .Build()
            .Build();
    }
}
