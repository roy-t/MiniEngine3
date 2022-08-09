using System.Collections;
using System.Reflection;

namespace Mini.Engine.Configuration;

public sealed class ComponentCatalog : IEnumerable<Type>
{
    public ComponentCatalog(IEnumerable<Assembly> assemblies)
    {
        var components = new List<Type>();
        foreach (var assembly in assemblies)
        {
            components.AddRange(this.Scan(assembly));
        }

        this.ComponentTypes = components;
    }

    public IReadOnlyList<Type> ComponentTypes { get; }

    private IEnumerable<Type> Scan(Assembly assembly)
    {
        return assembly.DefinedTypes.Where(info => IsComponentType(info)).Cast<Type>();
    }

    private static bool IsComponentType(TypeInfo typeInfo)
    {
        // TODO: remove class
        return (typeInfo.IsClass || typeInfo.IsValueType)
               && ImplementsComponentAttribute(typeInfo)
               && !typeInfo.IsNestedPrivate
               && !typeInfo.IsAbstract;
    }

    public static bool ImplementsComponentAttribute(TypeInfo typeInfo)
    {
        if (typeInfo.IsDefined(typeof(ComponentAttribute), true))
        {
            return true;
        }

        return typeInfo.GetInterfaces().Any(t => t.IsDefined(typeof(ComponentAttribute), true));
    }

    public int Count => this.ComponentTypes.Count;

    public IEnumerator<Type> GetEnumerator()
    {
        return this.ComponentTypes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.ComponentTypes.GetEnumerator();
    }
}
