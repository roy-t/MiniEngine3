namespace Mini.Engine.ECS.Components;

public enum LifeCycleState : byte
{
    Unchanged = 0,
    Changed,
    New,
    Removed
}

public readonly record struct LifeCycle(LifeCycleState Current, LifeCycleState Next)
{
    internal static LifeCycle Init()
    {
        return new LifeCycle(LifeCycleState.New, LifeCycleState.Unchanged);
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
        return this.Next switch
        {            
            LifeCycleState.Removed => this with { Current = this.Next, Next = LifeCycleState.Removed },
            // New, Changed, Unchanged, 
            _ => this with { Current = this.Next, Next = LifeCycleState.Unchanged },
        };
    }
}
