namespace Mini.Engine.Core.Lifetime;
public interface ILifetime
{
    int Id { get; }
    int Version { get; }    
}

public interface ILifetime<out T> : ILifetime
{

}

public readonly record struct StandardLifetime<T>(int Id, int Version): ILifetime<T>;
