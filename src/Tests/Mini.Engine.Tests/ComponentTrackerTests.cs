using Mini.Engine.Configuration;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
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
        var catalog = new ComponentCatalog(new[] { typeof(ComponentTrackerTests).Assembly });
        var tracker = new ComponentTracker(catalog);
              
        var bitA = tracker.GetBit<ComponentA>();
        Equal(1UL, bitA.Bit);

        var bitB = tracker.GetBit<ComponentB>();
        Equal(2UL, bitB.Bit);

        var entity = new Entity(1);

        tracker.SetComponent(entity, bitA);

        True(tracker.HasComponent(entity, bitA));
        False(tracker.HasComponent(entity, bitB));

        tracker.UnsetComponent(entity, bitA);
        False(tracker.HasComponent(entity, bitA));
    }
}
