namespace Mini.Engine.ECS.Experimental;

public interface IComponentContainer
{
    public Type ComponentType { get; }
}

public interface IComponentContainer<T> : IComponentContainer
    where T : struct, IComponent
{
    ref T this[Entity entity] { get; }
    ref T Create(Entity entity);
    void Remove(Entity entity);

    void UpdateLifeCycles();

    ResultIterator<T> Iterate(IQuery<T> query);
    ResultIterator<T> IterateAll();
    ResultIterator<T> IterateChanged();
    ResultIterator<T> IterateNew();
    ResultIterator<T> IterateRemoved();
    ResultIterator<T> IterateUnchanged();
}

public sealed class ComponentContainer<T> : IComponentContainer<T>
    where T : struct, IComponent
{
    public static readonly IQuery<T> AcceptAll = new QueryAll<T>();
    public static readonly IQuery<T> AcceptChanged = new QueryLifCcycle<T>(LifeCycleState.Changed);
    public static readonly IQuery<T> AcceptNew = new QueryLifCcycle<T>(LifeCycleState.New);
    public static readonly IQuery<T> AcceptRemoved = new QueryLifCcycle<T>(LifeCycleState.Removed);
    public static readonly IQuery<T> AcceptUnchanged = new QueryLifCcycle<T>(LifeCycleState.Unchanged);

    private const int InitialCapacity = 10;
    private readonly PoolAllocator<T> Pool;

    public ComponentContainer()
    {
        this.Pool = new PoolAllocator<T>(InitialCapacity);
    }

    public Type ComponentType => typeof(T);

    public ref T this[Entity entity] => ref this.Pool[entity];

    public ref T Create(Entity entity)
    {
        return ref this.Pool.CreateFor(entity);
    }

    public void Remove(Entity entity)
    {
        ref var component = ref this[entity];
        component.LifeCycle = component.LifeCycle.ToRemoved();
    }

    public void UpdateLifeCycles()
    {
        for (var i = 0; i < this.Pool.Count; i++)
        {
            ref var component = ref this.Pool[i];
            if (component.LifeCycle.Current == LifeCycleState.Removed)
            {
                this.Pool.Destroy(i);
            }
            else
            {
                component.LifeCycle = component.LifeCycle.ToNext();
            }
        }
    }

    public ResultIterator<T> Iterate(IQuery<T> query)
    {
        return new ResultIterator<T>(this.Pool, query);
    }

    public ResultIterator<T> IterateAll()
    {
        return new ResultIterator<T>(this.Pool, AcceptAll);
    }

    public ResultIterator<T> IterateChanged()
    {
        return new ResultIterator<T>(this.Pool, AcceptChanged);
    }

    public ResultIterator<T> IterateNew()
    {
        return new ResultIterator<T>(this.Pool, AcceptNew);
    }

    public ResultIterator<T> IterateRemoved()
    {
        return new ResultIterator<T>(this.Pool, AcceptRemoved);
    }

    public ResultIterator<T> IterateUnchanged()
    {
        return new ResultIterator<T>(this.Pool, AcceptUnchanged);
    }
}
