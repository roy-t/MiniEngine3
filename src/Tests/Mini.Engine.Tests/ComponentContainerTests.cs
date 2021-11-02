using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests
{
    public class ComponentContainerTests
    {
        private readonly struct Component : IComponent
        {
            public Component(int id)
            {
                this.Entity = new Entity(id);
                this.ChangeState = new ComponentChangeState();
            }

            public Entity Entity { get; }
            public ComponentChangeState ChangeState { get; }
        }

        private Component three = new(3);
        private Component five = new(5);
        private Component seven = new(7);
        private Component nine = new(9);

        [Fact]
        public void RemoveShouldMarkForRemoval()
        {
            var container = new ComponentContainer<Component>();

            container.Add(ref this.three);
            True(container.Contains(this.three.Entity));

            container.Flush();
            True(container.Contains(this.three.Entity));

            container.Remove(this.three.Entity);
            True(container.Contains(this.three.Entity));

            container.Flush();
            False(container.Contains(this.three.Entity));
        }
    }
}