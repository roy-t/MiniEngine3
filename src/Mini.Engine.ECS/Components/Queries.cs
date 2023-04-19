namespace Mini.Engine.ECS.Components;

public interface IQuery<T>
    where T : struct, IComponent
{
    public bool Accept(ref Component<T> entry);
}

public sealed class QueryAll<T>
    : IQuery<T>
    where T : struct, IComponent
{
    public bool Accept(ref Component<T> entry)
    {
        return
            entry.LifeCycle.Current != LifeCycleState.Created &&
            entry.LifeCycle.Current != LifeCycleState.Removed;
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

    public bool Accept(ref Component<T> entry)
    {
        return entry.LifeCycle.Current == this.State;
    }
}