using System;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics;

public sealed class ModelComponent : AComponent, IDisposable
{
    public ModelComponent(Entity entity, Model model)
        : base(entity)
    {
        this.Model = model;
    }

    public Model Model { get; }

    public void Dispose()
    {
        this.Model.Dispose();
    }
}
