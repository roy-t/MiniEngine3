using System;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Models;

public sealed class ModelComponent : Component
{
    public ModelComponent(Entity entity, IModel model)
        : base(entity)
    {
        this.Model = model;
    }

    public IModel Model { get; }
}
