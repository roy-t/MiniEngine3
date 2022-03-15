using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public sealed class SkyboxComponent : Component
{
    public SkyboxComponent(Entity entity, ITextureCube albedo, ITextureCube irradiance, ITextureCube environment, float strength)
        : base(entity)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.Strength = strength;
    }

    public ITextureCube Albedo { get; }
    public ITextureCube Irradiance { get; }
    public ITextureCube Environment { get; }
    public float Strength { get; }
}
