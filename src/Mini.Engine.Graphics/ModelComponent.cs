using System;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics;

public sealed class ModelComponent : Component, IDisposable
{
    public ModelComponent(Entity entity, IModel model)
        : base(entity)
    {
        this.Model = model;
    }

    public IModel Model { get; }

    public void Dispose()
    {
        this.Model.Dispose();
    }
}
