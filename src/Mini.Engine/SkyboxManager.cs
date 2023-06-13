using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;

namespace Mini.Engine;

[Service]
public sealed class SkyboxManager
{
    private readonly Device Device;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;
    private readonly CubeMapGenerator CubeMapGenerator;
    private readonly IComponentContainer<SkyboxComponent> Skyboxes;

    public SkyboxManager(Device device, ContentManager content, ECSAdministrator administrator, CubeMapGenerator cubeMapGenerator, IComponentContainer<SkyboxComponent> skyboxes)
    {
        this.Device = device;
        this.Content = content;
        this.Administrator = administrator;
        this.CubeMapGenerator = cubeMapGenerator;
        this.Skyboxes = skyboxes;
    }

    public void SetCircusSkybox()
    {
        this.SetSkybox(@"Skyboxes\circus.hdr");
    }

    public void SetHillyTerrainSkybox()
    {
        this.SetSkybox(@"Skyboxes\hilly_terrain.hdr");
    }

    public void SetSkybox(string path)
    {
        if (!this.Skyboxes.IsEmpty)
        {            
            foreach (ref var component in this.Skyboxes.IterateAll())
            {
                component.LifeCycle = component.LifeCycle.ToRemoved();
            }
        }
        
        var sky = this.Administrator.Entities.Create();
        var texture = this.Content.LoadTexture(path, new TextureSettings(SuperCompressed.Mode.Linear, true, true));
        var albedo = this.CubeMapGenerator.GenerateAlbedo(texture, sky.ToString());
        var irradiance = this.CubeMapGenerator.GenerateIrradiance(texture, sky.ToString());
        var environment = this.CubeMapGenerator.GenerateEnvironment(texture, sky.ToString());

        var levels = this.Device.Resources.Get(environment).MipMapLevels;

        ref var skybox = ref this.Administrator.Components.Create<SkyboxComponent>(sky);
        skybox.Init(albedo, irradiance, environment, levels, 0.1f);
    }
}
