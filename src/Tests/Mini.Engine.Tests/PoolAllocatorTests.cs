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

        public Entity Entity { get; set; }
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
        Equal(10, allocator.Capacity);


        ref var component = ref allocator.CreateFor(entity);
        component.Value = 1;

        Equal(1, allocator.Count);
        Equal(1, allocator[0].Value);

        allocator.DestroyFor(entity);
        Equal(0, allocator.Count);
        Equal(-1, component.Value);

        Equal(10, allocator.Capacity);
        allocator.Reserve(100);
        Equal(100, allocator.Capacity);
        allocator.Trim();
        Equal(0, allocator.Capacity);

        for (var i = 0; i < 100; i++)
        {
            ref var c = ref allocator.CreateFor(new Entity(i));
            c.Value = i;
        }

        Equal(100, allocator.Count);
        True(allocator.Capacity >= 100);

        allocator.Trim();
        Equal(100, allocator.Capacity);

        allocator.Destroy(50);
        Equal(99, allocator.Count);

        True(allocator[50].Value >= 0);
    }
}
