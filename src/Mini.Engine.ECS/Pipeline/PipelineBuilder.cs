using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Systems;
using Serilog;

namespace Mini.Engine.ECS.Pipeline;

[Service]
public sealed class PipelineBuilder
{

    private readonly ILogger Logger;

    private readonly Services Services;
    private readonly ContainerStore ContainerStore;

    public PipelineBuilder(Services services, ContainerStore containerStore, ILogger logger)
    {
        this.Services = services;
        this.ContainerStore = containerStore;
        this.Logger = logger.ForContext<PipelineBuilder>();
    }

    public PipelineSpecifier Builder() => new(this.Services, this.ContainerStore, this.Logger);

    public class PipelineSpecifier
    {
        private readonly Services Services;
        private readonly ContainerStore ContainerStore;
        private readonly ILogger Logger;
        private readonly List<SystemSpec> SystemSpecs;

        public PipelineSpecifier(Services services, ContainerStore containerStore, ILogger logger)
        {
            this.SystemSpecs = new List<SystemSpec>();
            this.Services = services;
            this.ContainerStore = containerStore;
            this.Logger = logger.ForContext<PipelineSpecifier>();
        }

        public SystemSpecifier System<T>()
            where T : ISystem
        {
            var spec = SystemSpec.Construct<T>();
            this.SystemSpecs.Add(spec);

            return new SystemSpecifier(this, spec);
        }

        public ParallelPipeline Build()
        {
            var stages = SystemSpecOrderer.DivideIntoStages(this.SystemSpecs);

            var pipelineStages = new List<PipelineStage>();
            for (var i = 0; i < stages.Count; i++)
            {
                var systemBindings = new List<ISystemBinding>();
                var stage = stages[i];
                for (var j = 0; j < stage.Count; j++)
                {
                    var systemSpec = stage[j];
                    var system = this.Services.Resolve<ISystem>(systemSpec.SystemType);
                    var binding = this.Services.Resolve<ISystemBinding>(system.GetSystemBindingType());
                    systemBindings.Add(binding);
                }

                pipelineStages.Add(new PipelineStage(systemBindings));
            }

            var text = new StringBuilder();
            _ = text.AppendLine("Parallel Pipeline:");
            PrintPipeline(text, pipelineStages);

            this.Logger.Information(text.ToString());

            return new ParallelPipeline(pipelineStages);
        }

        private static void PrintPipeline(StringBuilder text, IReadOnlyList<PipelineStage> stages)
        {
            for (var i = 0; i < stages.Count; i++)
            {
                var stage = stages[i];
                _ = text.Append($"[{i}]: ")
                    .AppendJoin(", ", stage.Systems.Select(system => system.GetType().Name))
                    .AppendLine();
            }
        }
    }

    public class SystemSpecifier
    {
        private readonly PipelineSpecifier Parent;
        private readonly SystemSpec Spec;

        public SystemSpecifier(PipelineSpecifier parent, SystemSpec spec)
        {
            this.Parent = parent;
            this.Spec = spec;
        }

        public SystemSpecifier Requires(string resource, string state)
        {
            _ = this.Spec.Requires(resource, state);
            return this;
        }

        public SystemSpecifier Requires(Enum resource, Enum state)
            => this.Requires(resource.ToString(), state.ToString());

        public SystemSpecifier RequiresAll(string resource)
        {
            _ = this.Spec.RequiresAll(resource);
            return this;
        }

        public SystemSpecifier RequiresAll(Enum resource)
            => this.RequiresAll(resource.ToString());

        public SystemSpecifier Produces(string resource, string state)
        {
            _ = this.Spec.Produces(resource, state);
            return this;
        }

        public SystemSpecifier Produces(Enum resource, Enum state)
            => this.Produces(resource.ToString(), state.ToString());

        public SystemSpecifier Produces(string resource)
        {
            _ = this.Spec.Produces(resource);
            return this;
        }

        public SystemSpecifier Produces(Enum resource)
            => this.Produces(resource.ToString());

        public SystemSpecifier Parallel()
        {
            _ = this.Spec.Parallel();
            return this;
        }

        public SystemSpecifier InSequence()
        {
            _ = this.Spec.InSequence();
            return this;
        }

        public PipelineSpecifier Build()
            => this.Parent;
    }
}
