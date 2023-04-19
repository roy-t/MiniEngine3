using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public struct SkyboxComponent : IComponent
{
    public ILifetime<IRenderTargetCube> Albedo;
    public ILifetime<IRenderTargetCube> Irradiance;
    public ILifetime<IRenderTargetCube> Environment;
    public float Strength;
    public int EnvironmentLevels;

    public void Init(ILifetime<IRenderTargetCube> albedo, ILifetime<IRenderTargetCube> irradiance, ILifetime<IRenderTargetCube> environment, int environmentLevels, float strength)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.EnvironmentLevels = environmentLevels;
        this.Strength = strength;
    }
}
