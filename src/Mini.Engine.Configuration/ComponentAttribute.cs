using System;

namespace Mini.Engine.Configuration;

/// <summary>
/// Marks the class as a component, the injector will make sure a suitable container is created
/// for the component
/// </summary>
/// <seealso cref="Injector"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = true)]
public sealed class ComponentAttribute : InjectableAttribute
{
}
