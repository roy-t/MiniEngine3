using System;
using System.Numerics;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Lighting.PointLights;

public sealed class PointLightComponent : Component
{
    private const float MinimumLightInfluence = 0.001f;

    private float strength;

    public PointLightComponent(Entity entity, Vector4 color, float strength)
        : base(entity)
    {
        this.Color = color;
        this.Strength = strength;
    }

    public Vector4 Color { get; set; }

    public float Strength
    {
        get => this.strength;
        set
        {
            this.strength = value;
            this.RadiusOfInfluence = MathF.Sqrt(value / MinimumLightInfluence);
        }
    }

    /// <summary>
    /// The maximum at which this light affects its surroundings. 
    /// Changes when the strength of the light changes
    /// </summary>
    public float RadiusOfInfluence { get; private set; }
}
