using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Models;

public struct ModelComponent : IComponent
{
    public IResource<IModel> Model;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
