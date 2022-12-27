using Mini.Engine.Configuration;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Hexagons;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Graphics.Vegetation;
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

        // TODO: having dependencies is nice, but its still unclear what happens first
        // maybe just manually define stages and set for each stage if it runs in parallel or not?

        return pipeline        
            .System<ClearBuffersSystem>()
                .InSequence()
                .Produces("Initialization", "GBuffer")
                .Build()
            .System<ModelSystem>()
                .InSequence()
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Models")
                .Build()
            .System<TerrainSystem>()
                .InSequence()                
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Terrain")
                .Build()
            .System<HexagonSystem>()
                .InSequence()
                .Requires("Initialization", "GBuffer")
                .Produces("Renderer", "Hexagons")
                .Build()
            .System<GrassSystem>()
                .InSequence()                
                .Requires("Initialization", "GBuffer")
                .Requires("Renderer", "Terrain")
                .Requires("Renderer", "Hexagons")
                .Produces("Renderer", "Grass")
                .Build()
            .System<CascadedShadowMapSystem>()
                .InSequence()                
                .Produces("Shadows", "CascadedShadowMap")
                .Build()
            .System<PointLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Requires("Renderer", "Hexagons")
                .Requires("Renderer", "Grass")
                .Produces("Renderer", "PointLights")
                .Build()
            .System<ImageBasedLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Requires("Renderer", "Hexagons")
                .Requires("Renderer", "Grass")
                .Produces("Renderer", "ImageBasedLights")
                .Build()
            .System<SunLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Requires("Renderer", "Hexagons")
                .Requires("Renderer", "Grass")
                .Requires("Shadows", "CascadedShadowMap")
                .Produces("Renderer", "SunLights")
                .Build()
            .System<SkyboxSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "Terrain")
                .Requires("Renderer", "Hexagons")
                .Requires("Renderer", "Grass")
                .Requires("Renderer", "PointLights")
                .Requires("Renderer", "ImageBasedLights")
                .Requires("Renderer", "SunLights")
                .Produces("Renderer", "Skybox")
                .Build()
            .System<PostProcessingSystem>()
                .InSequence()
                .RequiresAll("Renderer")
                .Produces("PostProcessing", "Post")
                .Build()
        .Build();
    }
}
