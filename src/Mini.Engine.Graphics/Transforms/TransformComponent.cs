using LibGame.Physics;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Transforms;

public struct TransformComponent : IComponent
{    
    public Transform Current;
    public Transform Previous;
}
