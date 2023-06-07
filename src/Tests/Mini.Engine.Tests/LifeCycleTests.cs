using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests;

public class LifeCycleTests
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
    public void LifeCycleTest()
    {
        var entity = new Entity(1);
        var entry = new Component(entity, 1)
        {
            LifeCycle = LifeCycle.Init(),
            Entity = entity
        };

        Equal(LifeCycleState.Created, entry.LifeCycle.Current);
        Equal(LifeCycleState.New, entry.LifeCycle.Next);

        entry.LifeCycle = entry.LifeCycle.ToNext();
        Equal(LifeCycleState.New, entry.LifeCycle.Current);
        Equal(LifeCycleState.Unchanged, entry.LifeCycle.Next);

        entry.LifeCycle = entry.LifeCycle.ToChanged();
        Equal(LifeCycleState.Changed, entry.LifeCycle.Next);
    }
}
