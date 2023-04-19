using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Models;

public struct ModelComponent : IComponent
{
    public ILifetime<IModel> Model;
}
