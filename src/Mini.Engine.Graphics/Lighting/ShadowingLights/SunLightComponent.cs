using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public struct SunLightComponent : IComponent
{    
    public Color4 Color { get; set; }
    public float Strength { get; set; }

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy()
    {
        
    }
}
