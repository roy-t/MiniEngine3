using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Scenes;

[Service]
public sealed class SponzaScene
{
    private readonly ContentManager Content;
    private readonly EntityAdministrator Entities;
    private readonly ComponentAdministrator Components;
    private readonly CubeMapGenerator CubeMapGenerator;

    public SponzaScene(ContentManager content, EntityAdministrator entities, ComponentAdministrator components, CubeMapGenerator cubeMapGenerator)
    {
        this.Content = content;
        this.Entities = entities;
        this.Components = components;
        this.CubeMapGenerator = cubeMapGenerator;
    }

    public string Title => "Sponza";

    public void Load()
    {
        this.Content.Push("Scene");

        var entity = this.Entities.Create();
        //components.Add(new ModelComponent(entity, content.LoadCube()));
        //components.Add(new TransformComponent(entity).SetScale(1.0f));
        this.Components.Add(new ModelComponent(entity, this.Content.LoadSponza()));
        this.Components.Add(new TransformComponent(entity).SetScale(0.01f));
        //models.Add(new ModelComponent(entity, content.LoadAsteroid()));

        var sphere = this.Entities.Create();
        //models.Add(new ModelComponent(sphere, SphereGenerator.Generate(device, 3, content.LoadDefaultMaterial(), "Sphere")));
        this.Components.Add(new PointLightComponent(sphere, Vector4.One, 100.0f));
        this.Components.Add(new TransformComponent(sphere));//.ApplyTranslation(Vector3.One * 10));

        var sky = this.Entities.Create();
        //var texture = content.LoadTexture(@"Skyboxes\industrial.hdr");
        var texture = this.Content.LoadTexture(@"Skyboxes\circus.hdr");
        var albedo = this.CubeMapGenerator.GenerateAlbedo(texture, false, "skybox_albedo");
        var irradiance = this.CubeMapGenerator.GenerateIrradiance(texture, "skybox_irradiance");
        var environment = this.CubeMapGenerator.GenerateEnvironment(texture, "skybox_environment");

        // Make sure the items are disposed whenever this content frame is
        this.Content.Link(albedo, albedo.Name);
        this.Content.Link(irradiance, irradiance.Name);
        this.Content.Link(environment, environment.Name);

        this.Components.Add(new SkyboxComponent(sky, albedo, irradiance, environment, 1.0f));
    }

    public void Unload()
    {
        this.Content.Pop();
    }
}
