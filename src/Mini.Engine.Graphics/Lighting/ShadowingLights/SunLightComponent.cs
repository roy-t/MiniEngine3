using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;
public struct SunLightComponent : IComponent
{
    public Color4 Color;
    public float Strength;
}
