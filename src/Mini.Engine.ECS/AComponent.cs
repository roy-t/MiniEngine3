using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.ECS
{
    [Component]
    public abstract class AComponent
    {
        protected AComponent(Entity entity)
        {
            this.Entity = entity;
            this.ChangeState = ComponentChangeState.NewComponent();
        }

        public Entity Entity { get; }

        public ComponentChangeState ChangeState { get; }
    }
}
