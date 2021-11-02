using System;

namespace Mini.Engine.Configuration
{
    /// <summary>
    /// Marks the struct as a component, the injector will make sure a suitable container is created
    /// for the component
    /// </summary>
    /// <seealso cref="Injector"/>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ComponentAttribute : Attribute
    {
    }
}
