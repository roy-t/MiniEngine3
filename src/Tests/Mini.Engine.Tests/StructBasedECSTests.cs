using Mini.Engine.ECS;
using Mini.Engine.ECS.Experimental;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;

public class StructBasedECSTests
{
    public struct Component : IStructComponent
    {
        public int ContainerIndex { get; set; }
        public int MyValue { get; set; }
    }

    [Fact]
    public void ContainerTest()
    {
        var container = new StructComponentContainer<Component>();

        Equal(0, container.Count);
        Equal(0.0f, container.Fragmentation);

        ref var component = ref container.Create();
        component.MyValue = 300;

        var hit = false;
        foreach(var c in container.EnumerateAll())
        {
            hit = true;
            Equal(300, c.MyValue);
        }

        True(hit);

        hit = false;
        foreach (var c in container.Enumerate(LifeCycle.Created))
        {
            hit = true;
            Equal(300, c.MyValue);
        }

        True(hit);

        container.AdvanceLifeCycles();

        hit = false;
        foreach (var c in container.Enumerate(LifeCycle.New))
        {
            hit = true;
            Equal(300, c.MyValue);
        }

        True(hit);

        Equal(1, container.Count);
        

        for(var i = 0; i < 15; i++)
        {
            ref var next = ref container.Create();
        }

        Equal(16, container.Count);

        // TODO: now destroy a few?
    }
}
