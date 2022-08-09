using System.Numerics;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.PointLights;

public struct PointLightComponent : IComponent
{
    private const float MinimumLightInfluence = 0.001f;

    private float strength;

    public void Init(Vector4 color, float strength)
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

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy() {  }
}
