using System.Numerics;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.PointLights;

public struct PointLightComponent : IComponent
{
    public float Strength;
    public Vector4 Color;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy() {  }
}
