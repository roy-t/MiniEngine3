using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public sealed class SkyboxComponent : Component
{
    public SkyboxComponent(Entity entity, ITexture2D albedo, ITexture2D irradiance, ITexture2D environment, float strength)
        : base(entity)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.Strength = strength;
    }

    public ITexture2D Albedo { get; }
    public ITexture2D Irradiance { get; }
    public ITexture2D Environment { get; }
    public float Strength { get; }
}
