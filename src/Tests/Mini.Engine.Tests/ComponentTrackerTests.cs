using Mini.Engine.ECS;
using Mini.Engine.ECS.Experimental;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;
public class ComponentTrackerTests
{
    public struct ComponentA : IComponent
    {
        public ComponentA(Entity entity)
        {
            this.LifeCycle = new LifeCycle();
            this.Entity = entity;
        }

        public LifeCycle LifeCycle { get; set; }
        public Entity Entity { get; set; }

        public void Destroy() { }
    }

    public struct ComponentB : IComponent
    {
        public ComponentB(Entity entity)
        {
            this.LifeCycle = new LifeCycle();
            this.Entity = entity;
        }

        public LifeCycle LifeCycle { get; set; }
        public Entity Entity { get; set; }

        public void Destroy() { }
    }

    [Fact]
    public void SmokeTest()
    {
        var containerA = (IComponentContainer)new ComponentContainer<ComponentA>();
        var containerB = (IComponentContainer)new ComponentContainer<ComponentB>();

        var tracker = new ComponentTracker(new[] { containerA, containerB });

        var bitA = tracker.GetBit<ComponentA>();
        Equal(1UL, bitA.Bit);

        var bitB = tracker.GetBit<ComponentB>();
        Equal(2UL, bitB.Bit);

        var entity = new Entity(1);

        ComponentTracker.SetComponent(ref entity, bitA);

        True(ComponentTracker.HasComponent(entity, bitA));
        False(ComponentTracker.HasComponent(entity, bitB));

        ComponentTracker.UnsetComponent(ref entity, bitA);
        False(ComponentTracker.HasComponent(entity, bitA));
    }
}
