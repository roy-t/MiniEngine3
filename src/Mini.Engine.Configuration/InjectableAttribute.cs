using System;

namespace Mini.Engine.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public abstract class InjectableAttribute : Attribute
{
}
