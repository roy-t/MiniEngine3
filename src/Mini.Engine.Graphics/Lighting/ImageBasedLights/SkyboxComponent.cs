using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public struct SkyboxComponent : IComponent
{
    public IResource<ITexture2D> Albedo;
    public IResource<ITexture2D> Irradiance;
    public IResource<ITexture2D> Environment;
    public float Strength;
    public int EnvironmentLevels;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(IResource<ITexture2D> albedo, IResource<ITexture2D> irradiance, IResource<ITexture2D> environment, int environmentLevels, float strength)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.EnvironmentLevels = environmentLevels;
        this.Strength = strength;
    }
}
