using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;
public class PoolAllocatorTests
{
    public struct Component : IComponent
    {
        public Component(Entity entity, int value)
        {
            this.LifeCycle = new LifeCycle();
            this.Entity = entity;
            this.Value = value;
        }

        public LifeCycle LifeCycle { get; set; }
        public Entity Entity { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return $"{this.Value}, {this.Entity}";
        }
    }

    [Fact]
    public void SmokeTest()
    {
        var entity = new Entity(1);
        var allocator = new ComponentPool<Component>(10);
        Equal(10, allocator.Capacity);


        ref var component = ref allocator.CreateFor(entity).Value;
        component.Value = 1;

        Equal(1, allocator.Count);
        Equal(1, allocator[0].Value.Value);

        allocator.DestroyFor(entity);
        Equal(0, allocator.Count);
        Equal(0, component.Value);

        Equal(10, allocator.Capacity);
        allocator.Resize(100);
        Equal(100, allocator.Capacity);
        allocator.Trim();
        Equal(0, allocator.Capacity);

        for (var i = 0; i < 100; i++)
        {
            ref var c = ref allocator.CreateFor(new Entity(i)).Value;
            c.Value = i;
        }

        Equal(100, allocator.Count);
        True(allocator.Capacity >= 100);

        allocator.Trim();
        Equal(100, allocator.Capacity);

        allocator.Destroy(50);
        Equal(99, allocator.Count);

        True(allocator[50].Value.Value >= 0);
    }

    [Fact]
    public void LifeCycleTest()
    {
        var entity = new Entity(1);
        var allocator = new ComponentPool<Component>(10);

        ref var entry = ref allocator.CreateFor(entity);
        Equal(LifeCycleState.Created, entry.LifeCycle.Current);
        Equal(LifeCycleState.New, entry.LifeCycle.Next);

        entry.LifeCycle = entry.LifeCycle.ToNext();
        Equal(LifeCycleState.New, entry.LifeCycle.Current);
        Equal(LifeCycleState.Unchanged, entry.LifeCycle.Next);

        entry.LifeCycle = entry.LifeCycle.ToChanged();
        Equal(LifeCycleState.Changed, entry.LifeCycle.Next);

        ref var component2 = ref allocator[0];
        Equal(LifeCycleState.Changed, component2.LifeCycle.Next);
    }
}
