using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

public struct SkyboxComponent : IComponent
{
    public IResource<IRenderTargetCube> Albedo;
    public IResource<IRenderTargetCube> Irradiance;
    public IResource<IRenderTargetCube> Environment;
    public float Strength;
    public int EnvironmentLevels;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(IResource<IRenderTargetCube> albedo, IResource<IRenderTargetCube> irradiance, IResource<IRenderTargetCube> environment, int environmentLevels, float strength)
    {
        this.Albedo = albedo;
        this.Irradiance = irradiance;
        this.Environment = environment;
        this.EnvironmentLevels = environmentLevels;
        this.Strength = strength;
    }
}
