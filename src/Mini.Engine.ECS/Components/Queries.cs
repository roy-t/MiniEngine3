namespace Mini.Engine.ECS.Components;

public interface IQuery<T>
    where T : struct, IComponent
{
    public bool Accept(ref T component);
}

public sealed class QueryAll<T>
    : IQuery<T>
    where T : struct, IComponent
{
    public bool Accept(ref T component)
    {
        return component.LifeCycle.Current != LifeCycleState.Removed;
    }
}

public sealed class QueryLifCcycle<T>
    : IQuery<T>
    where T : struct, IComponent
{
    private readonly LifeCycleState State;

    public QueryLifCcycle(LifeCycleState state)
    {
        this.State = state;
    }

    public bool Accept(ref T component)
    {
        return component.LifeCycle.Current == this.State;
    }
}