using System;
using System.Collections.Generic;
using System.Linq;

namespace Mini.Engine.Configuration;

public interface IRelationDescriber<TProducerConsumer, TProduct>
{
    IReadOnlyList<TProduct> GetConsumption(TProducerConsumer item);
    IReadOnlyList<TProduct> GetProduction(TProducerConsumer item);
}

/// <summary>
/// Produces the optimal ordering in which to produce/consume items so that producers/consumers
/// have to wait as little as possible on each other
/// </summary>
public sealed class CoffmanGraham<TProducerConsumer, TProduct>
{
    private readonly IRelationDescriber<TProducerConsumer, TProduct> Relations;

    public CoffmanGraham(IRelationDescriber<TProducerConsumer, TProduct> relations)
    {
        this.Relations = relations;
    }

    public IReadOnlyList<TProducerConsumer> Order(IEnumerable<TProducerConsumer> items)
    {
        var ordered = new List<TProducerConsumer>();
        var unordered = new List<TProducerConsumer>();

        foreach (var item in items)
        {
            var dependencies = this.Relations.GetConsumption(item);
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

    private bool GetNextCandidate(IReadOnlyList<TProducerConsumer> unordered, IReadOnlyList<TProducerConsumer> ordered, out TProducerConsumer candidate)
    {
        var maxDistance = int.MinValue;
        candidate = default!;

        foreach (var item in unordered)
        {
            // The best candidate has the largest distance from the items that produce their
            // requirements so that it does not have to wait long for what it needs.
            var minDistance = int.MaxValue;
            var dependencies = this.Relations.GetConsumption(item);
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

    private bool DistanceToRelease(TProduct dependency, IReadOnlyList<TProducerConsumer> ordered, out int distance)
    {
        for (var i = 0; i < ordered.Count; i++)
        {
            var item = ordered[i];
            var releases = this.Relations.GetProduction(item);
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
