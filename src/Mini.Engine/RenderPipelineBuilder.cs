using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.PBR;

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
            .System<ClearBuffersSystem>()
                .InSequence()
                .Produces("Initialization", "GBuffer")
                .Build()
            .System<ModelSystem>()
                .InSequence()
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Models")
                .Build()
            .System<PointLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Produces("Renderer", "PointLights")
                .Build()
            .System<SkyboxSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "PointLights")
                .Produces("Renderer", "Skybox")
                .Build()
        .Build();
    }
}
