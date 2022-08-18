using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.vNext;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public struct SkyboxComponent : IComponent
{
    public IResource<ITexture> Albedo;
    public IResource<ITexture> Irradiance;
    public IResource<ITexture> Environment;
    public float Strength;
    public int EnvironmentLevels;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(IResource<ITexture> albedo, IResource<ITexture> irradiance, IResource<ITexture> environment, int environmentLevels, float strength)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.EnvironmentLevels = environmentLevels;
        this.Strength = strength;
    }
}
