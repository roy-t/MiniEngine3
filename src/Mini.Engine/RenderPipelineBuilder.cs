﻿using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Pipeline;

namespace Mini.Engine
{
    [Service]
    internal sealed class RenderPipelineBuilder
    {
        private readonly PipelineBuilder Builder;

        public RenderPipelineBuilder(PipelineBuilder builder) => this.Builder = builder;

        public ParallelPipeline Build()
        {
            var pipeline = this.Builder.Builder();
            return pipeline
                .System<ComponentFlushSystem>()
                .Parallel()
                .Produces("Initialization", "Containers")
                .Build()
            .Build();
        }
    }
}
