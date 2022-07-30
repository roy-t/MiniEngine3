using Mini.Engine.ECS;
using Mini.Engine.ECS.Experimental;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;
public class PoolAllocatorTests
{
    public struct Component : IComponent
    {
        public Component(Entity entity, int value)
        {
            this.Entity = entity;
            this.Value = value;
        }

        public Entity Entity { get; }
        public int Value { get; set; }

        public void Destroy()
        {
            this.Value = -1;
        }

        public override string ToString()
        {
            return $"{this.Value}, {this.Entity}";
        }
    }

    [Fact]
    public void SmokeTest()
    {
        var entity = new Entity(1);

        var allocator = new PoolAllocator<Component>(10);

        ref var component = ref allocator.CreateFor(entity);
        component.Value = 1;

        True(allocator.IsOccupied(0));
        Equal(1, allocator.Count);
        Equal(1, allocator[0].Value);

        allocator.DestroyFor(entity);

        Equal(0, allocator.Count);
        False(allocator.IsOccupied(0));

        for (var i = 0; i < 20; i++)
        {
            var e = new Entity(i + 1000);
            ref var c = ref allocator.CreateFor(e);
            c.Value = i;
        }

        Equal(20, allocator.Count);
        True(allocator.Capacity >= 20);

        for (var i = 0; i < 20; i += 2)
        {
            var e = allocator[i].Entity;
            allocator.DestroyFor(e);
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
