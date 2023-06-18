using System.Diagnostics;
using LibGame.Collections;

namespace Mini.Engine.ECS.Components;

public interface IComponentContainer
{
    public Type ComponentType { get; }
    public bool Contains(Entity entity);
    void Remove(Entity entity);
    void UpdateLifeCycles();
    bool IsEmpty { get; }
}

public interface IComponentContainer<T> : IComponentContainer
    where T : struct, IComponent
{
    ref Component<T> this[Entity entity] { get; }
    ref T Create(Entity entity);

    ResultIterator<T> Iterate(IQuery<T> query);
    ResultIterator<T> IterateAll();
    ResultIterator<T> IterateChanged();
    ResultIterator<T> IterateNew();
    ResultIterator<T> IterateRemoved();
    ResultIterator<T> IterateUnchanged();

    EntityIterator<T> IterateEntities(IQuery<T> query);
    EntityIterator<T> IterateAllEntities();

    ref Component<T> First(IQuery<T> query);
}

public sealed class ComponentContainer<T> : IComponentContainer<T>
    where T : struct, IComponent
{
    public static readonly IQuery<T> AcceptAll = new QueryAll<T>();
    public static readonly IQuery<T> AcceptNew = new QueryLifeCycle<T>(LifeCycleState.New);
    public static readonly IQuery<T> AcceptUnchanged = new QueryLifeCycle<T>(LifeCycleState.Unchanged);
    public static readonly IQuery<T> AcceptChanged = new QueryLifeCycle<T>(LifeCycleState.Changed);

    public static readonly IQuery<T> AcceptRemoved = new QueryLifeCycle<T>(LifeCycleState.Removed);
    public static readonly IQuery<T> AcceptCreated = new QueryLifeCycle<T>(LifeCycleState.Created);

    private const int InitialCapacity = 10;

    private readonly ComponentTracker ComponentTracker;
    private readonly IndexTracker IndexTracker;
    private readonly StructPool<Component<T>> Pool;

    private readonly ComponentBit Bit;

    public ComponentContainer(ComponentTracker tracker)
    {
        this.Pool = new StructPool<Component<T>>(InitialCapacity);
        this.IndexTracker = new IndexTracker(InitialCapacity);

        this.Bit = tracker.GetBit();
        this.ComponentTracker = tracker;
    }

    public bool IsEmpty => this.Pool.IsEmpty;

    public Type ComponentType => typeof(T);

    public ref Component<T> this[Entity entity]
    {
        get
        {
            var index = this.IndexTracker.GetReference(entity);
            return ref this.Pool[index];
        }
    }

    public bool Contains(Entity entity)
    {
        return this.ComponentTracker.HasComponent(entity, this.Bit);
    }

    public ref T Create(Entity entity)
    {
        Debug.Assert(entity.Id > 0);

        this.ComponentTracker.SetComponent(entity, this.Bit);

        var index = this.Pool.Add(ComponentInitializer, entity);
        this.IndexTracker.InsertOrUpdate(entity, index);
        return ref this.Pool[index].Value;
    }

    public void Remove(Entity entity)
    {
        var index = this.IndexTracker.GetReference(entity);
        ref var entry = ref this.Pool[index];
        entry.LifeCycle = entry.LifeCycle.ToRemoved();
    }

    public void UpdateLifeCycles()
    {
        foreach (ref var component in this.Pool)
        {
            if (component.LifeCycle.Current == LifeCycleState.Removed)
            {
                this.ComponentTracker.UnsetComponent(component.Entity, this.Bit);
                var index = this.IndexTracker.GetReference(component.Entity);
                this.IndexTracker.Remove(component.Entity);

                this.Pool.Remove(index);
            }
            else
            {
                component.LifeCycle = component.LifeCycle.ToNext();
            }
        }
    }

    public ref Component<T> First(IQuery<T> query)
    {
        foreach (ref var component in this.Pool)
        {
            if (query.Accept(ref component))
            {
                return ref component;
            }
        }

        throw new NotSupportedException("Container does not contain at least one element matching the query");
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

    public EntityIterator<T> IterateEntities(IQuery<T> query)
    {
        return new EntityIterator<T>(this.Pool, query);
    }

    public EntityIterator<T> IterateAllEntities()
    {
        return new EntityIterator<T>(this.Pool, AcceptAll);
    }

    private static void ComponentInitializer(ref Component<T> component, Entity entity)
    {
        component.Entity = entity;
        component.LifeCycle = LifeCycle.Init();
    }
}
