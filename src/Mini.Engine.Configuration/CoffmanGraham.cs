using System;
using System.Collections.Generic;
using System.Linq;

namespace Mini.Engine.Configuration;


public interface IRelationDescriber<TConsumer, TConsumable>
{
    IReadOnlyList<TConsumable> GetConstraints(TConsumer item);
    IReadOnlyList<TConsumable> GetReleases(TConsumer item);
}

public sealed class CoffmanGraham<TConsumer, TConsumable>
{
    private readonly IRelationDescriber<TConsumer, TConsumable> Relations;

    public CoffmanGraham(IRelationDescriber<TConsumer, TConsumable> describer)
    {
        this.Relations = describer;
    }

    public IReadOnlyList<TConsumer> Order(IEnumerable<TConsumer> items)
    {
        var ordered = new List<TConsumer>();
        var unordered = new List<TConsumer>();

        foreach (var item in items)
        {
            var dependencies = this.Relations.GetConstraints(item);
            if (dependencies.Count == 0)
            {
                ordered.Add(item);
            }
            else
            {
                unordered.Add(item);
            }    
        }

        while (unordered.Count > 0)
        {

            if (this.GetNextCandidate(unordered, ordered, out var candidate))
            {
                ordered.Add(candidate);
                unordered.Remove(candidate);
            }
            else
            {
                var unresolved = string.Join(", ", unordered);
                throw new Exception($"Unsatisfiable dependency or cyle detected, could not order {unresolved}");
            }
        }

        return ordered;
    }

    private bool GetNextCandidate(IReadOnlyList<TConsumer> unordered, IReadOnlyList<TConsumer> ordered, out TConsumer candidate)
    {
        var maxDistance = int.MinValue;
        candidate = default!;

        foreach (var item in unordered)
        {
            // The best candidate has the largest distance from the items that produce their
            // requirements so that it does not have to wait long for what it needs.
            var minDistance = int.MaxValue;
            var dependencies = this.Relations.GetConstraints(item);
            foreach (var dependency in dependencies)
            {
                if (this.DistanceToRelease(dependency, ordered, out var distance))
                {
                    minDistance = Math.Min(distance, minDistance);
                }
                else
                {
                    goto NextCandidate;
                }
            }

            if (minDistance > maxDistance)
            {
                maxDistance = minDistance;
                candidate = item;
            }

        NextCandidate: { }
        }
       
        return maxDistance > int.MinValue;
    }

    private bool DistanceToRelease(TConsumable dependency, IReadOnlyList<TConsumer> ordered, out int distance)
    {
        for (var i = 0; i < ordered.Count; i++)
        {
            var item = ordered[i];
            var releases = this.Relations.GetReleases(item);
            if (releases.Contains(dependency))
            {
                distance = ordered.Count - i;
                return true;
            }
        }

        distance = int.MaxValue;
        return false;
    }
}
