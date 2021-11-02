using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Xunit;
using static Xunit.Assert;

namespace Mini.Engine.Tests
{
    public class SortedComponentListTests
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
        public void AddShouldAddElementAndKeepCompentsSorted()
        {
            var list = new SortedComponentList<Component>();

            Equal(0, list.Count);

            list.Add(ref this.five);

            Equal(1, list.Count);
            Equal(5, list[0].Entity.Id);

            list.Add(ref this.seven);
            Equal(2, list.Count);
            Equal(5, list[0].Entity.Id);
            Equal(7, list[1].Entity.Id);

            list.Add(ref this.nine);
            Equal(3, list.Count);
            Equal(5, list[0].Entity.Id);
            Equal(7, list[1].Entity.Id);
            Equal(9, list[2].Entity.Id);

            list.Add(ref this.three);
            Equal(4, list.Count);
            Equal(3, list[0].Entity.Id);
            Equal(5, list[1].Entity.Id);
            Equal(7, list[2].Entity.Id);
            Equal(9, list[3].Entity.Id);
        }

        [Fact]
        public void RemoveShouldRemoveRightElement()
        {
            var list = new SortedComponentList<Component>();

            list.Add(ref this.three);
            list.Add(ref this.five);
            list.Add(ref this.seven);
            list.Add(ref this.nine);

            list.Remove(new Entity(7));

            Equal(3, list.Count);
            Equal(3, list[0].Entity.Id);
            Equal(5, list[1].Entity.Id);
            Equal(9, list[2].Entity.Id);
        }

        [Fact]
        public void RemoveAtShouldRemoveRightElement()
        {
            var list = new SortedComponentList<Component>();

            list.Add(ref this.three);
            list.Add(ref this.five);
            list.Add(ref this.seven);
            list.Add(ref this.nine);

            list.RemoveAt(2);

            Equal(3, list.Count);
            Equal(3, list[0].Entity.Id);
            Equal(5, list[1].Entity.Id);
            Equal(9, list[2].Entity.Id);
        }
    }
}