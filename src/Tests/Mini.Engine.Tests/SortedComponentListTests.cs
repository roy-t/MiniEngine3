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
            }
            public Entity Entity { get; }
        }

        [Fact]
        public void AddShouldAddElementAndKeepCompentsSorted()
        {
            var list = new SortedComponentList<Component>();

            Equal(0, list.Count);

            list.Add(new Component(5));

            Equal(1, list.Count);
            Equal(5, list[0].Entity.Id);

            list.Add(new Component(7));
            Equal(2, list.Count);
            Equal(5, list[0].Entity.Id);
            Equal(7, list[1].Entity.Id);

            list.Add(new Component(9));
            Equal(3, list.Count);
            Equal(5, list[0].Entity.Id);
            Equal(7, list[1].Entity.Id);
            Equal(9, list[2].Entity.Id);

            list.Add(new Component(3));
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

            list.Add(new Component(3));
            list.Add(new Component(5));
            list.Add(new Component(7));
            list.Add(new Component(9));

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

            list.Add(new Component(3));
            list.Add(new Component(5));
            list.Add(new Component(7));
            list.Add(new Component(9));

            list.RemoveAt(2);

            Equal(3, list.Count);
            Equal(3, list[0].Entity.Id);
            Equal(5, list[1].Entity.Id);
            Equal(9, list[2].Entity.Id);
        }
    }
}