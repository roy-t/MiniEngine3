using System;
using System.Collections.Generic;
using System.Linq;
using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Pipeline;

/// <summary>
/// https://en.wikipedia.org/wiki/Coffman%E2%80%93Graham_algorithm
/// </summary>
public static class SystemSpecOrderer
{
    private class SystemRelationDescriber : IRelationDescriber<SystemSpec, ResourceState>
    {
        public IReadOnlyList<ResourceState> GetConsumption(SystemSpec item)
        {
            return item.RequiredResources;
        }

        public IReadOnlyList<ResourceState> GetProduction(SystemSpec item)
        {
            return item.ProducedResources;
        }
    }

    public static List<List<SystemSpec>> DivideIntoStages(List<SystemSpec> systemSpecs)
    {
        var expanded = ExpandRequirements(systemSpecs);

        var relations = new SystemRelationDescriber();
        var coffmanGraham = new CoffmanGraham<SystemSpec, ResourceState>(relations);
        var ordered = coffmanGraham.Order(expanded);
        var stages = CreateStages(ordered);

        //return SingleThreaded(stages);

        SplitStagesWithMixedParallelism(stages);
        return stages;
    }

    private static List<List<SystemSpec>> SingleThreaded(List<List<SystemSpec>> stages)
    {
        var single = new List<List<SystemSpec>>();

        foreach (var stage in stages)
        {
            foreach (var system in stage)
            {
                var sequentialStage = new List<SystemSpec>(new[] { system });
                single.Add(sequentialStage);
            }
        }
        return single;
    }   

    private static IReadOnlyList<SystemSpec> ExpandRequirements(IReadOnlyList<SystemSpec> systemSpecs)
    {
        var producedResources = systemSpecs.SelectMany(s => s.ProducedResources)
                                           .GroupBy(r => r.Resource)
                                           .ToDictionary(gr => gr.Key, gr => gr.ToList());

        var expanded = new List<SystemSpec>();
        foreach (var spec in systemSpecs)
        {
            if (spec.RequiredResources.Any(r => r.SubResource == SystemSpec.MatchAllSubResources))
            {
                spec.ExpandRequiredResource(producedResources);
            }

            expanded.Add(spec);
        }
        return expanded;
    }

    private static List<List<SystemSpec>> CreateStages(IReadOnlyList<SystemSpec> orderedSystemSpecs)
    {
        var stages = new List<List<SystemSpec>>();
        var produced = new List<ResourceState>();

        var currentStage = new List<SystemSpec>();

        foreach (var systemSpec in orderedSystemSpecs)
        {
            if (AllRequirementsHaveBeenProduced(systemSpec, produced))
            {
                currentStage.Add(systemSpec);
            }
            else
            {
                produced.AddRange(GetProducedResource(currentStage));
                if (AllRequirementsHaveBeenProduced(systemSpec, produced))
                {
                    stages.Add(currentStage);
                    currentStage = new List<SystemSpec>() { systemSpec };
                }
                else
                {
                    throw new Exception("Algorithm error");
                }
            }
        }

        if (currentStage.Count > 0)
        {
            stages.Add(currentStage);
        }

        return stages;
    }

    private static void SplitStagesWithMixedParallelism(List<List<SystemSpec>> stages)
    {
        for (var i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];

            while (stage.Count > 1 && stage.Any(s => !s.AllowParallelism))
            {
                var sequentialSystem = stage.First(s => !s.AllowParallelism);
                stage.Remove(sequentialSystem);

                var sequentialStage = new List<SystemSpec>(new[] { sequentialSystem });
                stages.Insert(i++, sequentialStage);
            }
        }
    }

    private static bool AllRequirementsHaveBeenProduced(SystemSpec systemSpec, List<ResourceState> produced)
    {
        return systemSpec.RequiredResources.All(resource => produced.Contains(resource));
    }

    private static List<ResourceState> GetProducedResource(List<SystemSpec> systemSpecs)
    {
        return systemSpecs.SelectMany(systemSpec => systemSpec.ProducedResources).ToList();
    }
}
