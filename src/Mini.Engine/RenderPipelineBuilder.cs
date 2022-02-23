﻿using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
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
            .System<CascadedShadowMapSystem>()
                .InSequence()
                .Requires("Initialization", "Containers")
                .Produces("Shadows", "CascadedShadowMap")
                .Build()
            .System<PointLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Produces("Renderer", "PointLights")
                .Build()
            .System<ImageBasedLightSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Produces("Renderer", "ImageBasedLights")
                .Build()
            .System<SkyboxSystem>()
                .InSequence()
                .Requires("Renderer", "Models")
                .Requires("Renderer", "PointLights")
                .Requires("Renderer", "ImageBasedLights")
                .Produces("Renderer", "Skybox")
                .Build()
        .Build();
    }
}
