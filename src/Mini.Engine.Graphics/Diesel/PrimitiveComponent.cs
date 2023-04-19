using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;

public struct PrimitiveComponent : IComponent
{
    public ILifetime<PrimitiveMesh> Mesh;
    public Color4 Color;
}
