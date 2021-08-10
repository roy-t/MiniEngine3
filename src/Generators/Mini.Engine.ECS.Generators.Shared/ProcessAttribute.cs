using System;

namespace Mini.Engine.ECS.Generators.Shared
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProcessAttribute : Attribute
    {
        public ProcessQuery Query;
    }
}
