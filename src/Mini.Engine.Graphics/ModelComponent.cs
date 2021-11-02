using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics
{
    [Component]
    public struct ModelComponent : IComponent, IDisposable
    {
        public ModelComponent(Entity entity, Model model)
        {
            this.Entity = entity;
            this.ChangeState = new ComponentChangeState();

            this.Model = model;
        }

        public Model Model { get; }

        public Entity Entity { get; }

        public ComponentChangeState ChangeState { get; }

        public void Dispose()
        {
            this.Model.Dispose();
        }
    }
}
