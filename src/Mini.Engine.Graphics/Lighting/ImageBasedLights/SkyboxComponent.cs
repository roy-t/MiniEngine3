using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public struct SkyboxComponent : IComponent
{
    public void Init (ITexture2D albedo, ITexture2D irradiance, ITexture2D environment, float strength)        
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.Strength = strength;
    }

    public void Destroy() { }

    public ITexture2D Albedo { get; set; }
    public ITexture2D Irradiance { get; set; }
    public ITexture2D Environment { get; set; }
    public float Strength { get; set; }

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
