using System;
using System.Collections.Generic;
using System.Linq;

namespace Mini.Engine.Configuration;

public static class InjectableDependencies
{
    private class TypeRelationDescriber : IRelationDescriber<Type, Type>
    {
        public IReadOnlyList<Type> GetConsumption(Type item)
        {
            return GetDependencies(item);
        }

        public IReadOnlyList<Type> GetProduction(Type item)
        {
            return new List<Type>() { item };
        }
    }

    public static IReadOnlyList<Type> CreateInitializationOrder(Type root)
    {
        var all = GetAllDependencies(root);
        var relations = new TypeRelationDescriber();
        var coffmanGraham = new CoffmanGraham<Type, Type>(relations);

        return coffmanGraham.Order(all);
    }

    private static IReadOnlyList<Type> GetAllDependencies(Type root)
    {
        var all = new HashSet<Type>();
        var stack = new Stack<Type>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var item = stack.Pop();
            var dependencies = GetDependencies(item);

            all.UnionWith(dependencies);

            foreach (var dependency in dependencies)
            {
                stack.Push(dependency);
            }
        }

        return all.ToList();
    }

    private static IReadOnlyList<Type> GetDependencies(Type type)
    {
        var dependencies = new List<Type>();

        if (type.IsDefined(typeof(InjectableAttribute), true))
        {
            var constructor = type.GetConstructors();
            if (constructor.Any())
            {
                var parameters = constructor.Single().GetParameters();
                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;
                    if (!parameterType.IsAbstract)
                    {
                        dependencies.Add(parameterType);
                    }
                }
            }
        }

        return dependencies;
    }
}
