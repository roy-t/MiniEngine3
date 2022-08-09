using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.World;

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
            .System<ComponentLifeCycleSystem>()
                .Parallel()
                .Produces("Initialization", "Containers")
                .Build()
            .System<ClearBuffersSystem>()
                .InSequence()
                .Produces("Initialization", "GBuffer")
                .Build()
            .System<ModelSystem>()
                .InSequence()
                .Requires("Initialization", "Containers")
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Models")
                .Build()
            .System<TerrainSystem>()
                .InSequence()
                .Requires("Initialization", "Containers")
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Terrain")
                .Build()
            .System<CascadedShadowMapSystem>()
                .InSequence()
                .Requires("Initialization", "Containers")
                .Produces("Shadows", "CascadedShadowMap")
                .Build()
            .System<PointLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Produces("Renderer", "PointLights")
                .Build()
            .System<ImageBasedLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Produces("Renderer", "ImageBasedLights")
                .Build()
            .System<SunLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Requires("Shadows", "CascadedShadowMap")
                .Produces("Renderer", "SunLights")
                .Build()
            .System<SkyboxSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Requires("Renderer", "PointLights")
                .Requires("Renderer", "ImageBasedLights")
                .Requires("Renderer", "SunLights")
                .Produces("Renderer", "Skybox")
                .Build()
        .Build();
    }
}
