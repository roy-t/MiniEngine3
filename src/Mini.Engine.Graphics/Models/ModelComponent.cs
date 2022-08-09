using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Models;

public struct ModelComponent : IComponent
{    
    public IModel Model { get; set; }

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy()
    {
        
    }
}
