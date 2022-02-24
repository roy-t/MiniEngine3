using Mini.Engine.ECS;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public sealed class SunLightComponent : Component
{
    public SunLightComponent(Entity entity, Color4 color, float strength)
        : base(entity)
    {
        this.Color = color;
        this.Strength = strength;
    }

    public Color4 Color { get; set; }
    public float Strength { get; set; }
}
