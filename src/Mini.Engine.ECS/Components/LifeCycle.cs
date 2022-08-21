namespace Mini.Engine.ECS.Components;

public enum LifeCycleState : byte
{
    Unchanged = 0,
    Changed,
    Created,
    New,
    Removed
}

public readonly record struct LifeCycle(LifeCycleState Current, LifeCycleState Next)
{
    internal static LifeCycle Init()
    {
        return new LifeCycle(LifeCycleState.Created, LifeCycleState.New);
    }

    public LifeCycle ToChanged()
    {
        return this with { Next = LifeCycleState.Changed };
    }

    public LifeCycle ToRemoved()
    {
        return this with { Next = LifeCycleState.Removed };
    }

    public LifeCycle ToNext()
    {
        return new LifeCycle(this.Next, LifeCycleState.Unchanged);
    }
}
