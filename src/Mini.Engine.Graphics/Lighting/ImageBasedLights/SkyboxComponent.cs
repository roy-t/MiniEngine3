using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public struct SkyboxComponent : IComponent
{
    public ITexture2D Albedo;
    public ITexture2D Irradiance;
    public ITexture2D Environment;
    public float Strength;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(ITexture2D albedo, ITexture2D irradiance, ITexture2D environment, float strength)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.Strength = strength;
    }

    public void Destroy() { }
}
