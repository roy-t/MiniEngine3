using System;
using System.Collections.Generic;
using System.Linq;

namespace Mini.Engine.Configuration;

public record DependencyNode(Type Type, List<DependencyNode> DependsOn);

public static class DependencyGraph
{
    public static DependencyNode Build(Type root)
    {
        var node = new DependencyNode(root, new List<DependencyNode>());
        var dependencies = GetDependencies(root);

        foreach(var dependency in dependencies)
        {
            node.DependsOn.Add(Build(dependency));
        }

        return node;
    }

    public static List<Type> CreateInitializationORder(DependencyNode root)
    {

    }

    private static IEnumerable<Type> GetDependencies(Type type)
    {
        if (type.IsDefined(typeof(InjectableAttribute), true))
        {
            var constructor = type.GetConstructors();
            if (constructor.Any())
            {
                return constructor.Single().GetParameters().Select(p => p.ParameterType).ToList();
            }
        }

        return Enumerable.Empty<Type>();
    }
}
