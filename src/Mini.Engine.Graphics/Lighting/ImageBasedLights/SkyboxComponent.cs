using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public sealed class SkyboxComponent : Component
{
    public SkyboxComponent(Entity entity, TextureCube albedo, TextureCube irradiance, TextureCube environment, float strength)
        : base(entity)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.Strength = strength;
    }

    public TextureCube Albedo { get; }
    public TextureCube Irradiance { get; }
    public TextureCube Environment { get; }
    public float Strength { get; }
}
