using Mini.Engine.ECS.Experimental;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;
public class PoolAllocatorTests
{
    public struct Component : IComponent
    {
        public int Value { get; set; }

        public void Destroy()
        {
            this.Value = -1;
        }

        public override string ToString()
        {
            return $"{this.Value}";
        }
    }

    [Fact]
    public void SmokeTest()
    {
        var allocator = new PoolAllocator<Component>(10);

        ref var component = ref allocator.Create();
        component.Value = 1;

        True(allocator.IsOccupied(0));
        Equal(1, allocator.Count);
        Equal(1, allocator[0].Value);

        allocator.Destroy(0);

        Equal(0, allocator.Count);
        False(allocator.IsOccupied(0));

        for (var i = 0; i < 20; i++)
        {
            ref var c = ref allocator.Create();
            c.Value = i;
        }

        Equal(20, allocator.Count);
        True(allocator.Capacity >= 20);
        Equal(0.0f, allocator.Fragmentation);

        for (var i = 0; i < 20; i += 2)
        {
            allocator.Destroy(i);
        }

        Equal(10, allocator.Count);        
        Equal(0.5f, allocator.Fragmentation);

        allocator.Vacuum();

        Equal(10, allocator.Count);        
        Equal(0.0f, allocator.Fragmentation);

        allocator.Reserve(500);
        Equal(500, allocator.Capacity);

        allocator.Vacuum();
        True(allocator.Capacity < 500);
    }
}
