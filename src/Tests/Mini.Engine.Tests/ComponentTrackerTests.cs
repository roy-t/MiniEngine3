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
        }

        public void Destroy() { }
    }

    public struct ComponentB : IComponent
    {
        public ComponentB(Entity entity)
        {            
        }

        public void Destroy() { }
    }

    [Fact]
    public void SmokeTest()
    {
        var tracker = new ComponentTracker();
              
        var bitA = tracker.GetBit();
        Equal(1UL, bitA.Bit);

        var bitB = tracker.GetBit();
        Equal(2UL, bitB.Bit);

        var entity = new Entity(1);

        tracker.SetComponent(entity, bitA);

        True(tracker.HasComponent(entity, bitA));
        False(tracker.HasComponent(entity, bitB));

        tracker.UnsetComponent(entity, bitA);
        False(tracker.HasComponent(entity, bitA));
    }
}
