using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
using Vortice.Mathematics;

namespace Mini.Engine.Scenes;

[Service]
public sealed class GeneratorScene : IScene
{
    private static readonly float[] Cascades =
{
        0.075f,
        0.15f,
        0.3f,
        1.0f
    };

    private readonly Device Device;
    private readonly ContentManager Content;
    private readonly EntityAdministrator Entities;
    private readonly ComponentAdministrator Components;
    private readonly CubeMapGenerator CubeMapGenerator;
    private readonly NoiseGenerator NoiseGenerator;
    private readonly HeightMapTriangulator Triangulator;

    private Entity? world;

    public GeneratorScene(Device device, ContentManager content, EntityAdministrator entities, ComponentAdministrator components, CubeMapGenerator cubeMapGenerator, NoiseGenerator noiseGenerator, HeightMapTriangulator triangulator)
    {
        this.Device = device;
        this.Content = content;
        this.Entities = entities;
        this.Components = components;
        this.CubeMapGenerator = cubeMapGenerator;
        this.NoiseGenerator = noiseGenerator;
        this.Triangulator = triangulator;
    }

    public string Title => "Generator";

    public IReadOnlyList<LoadAction> Load()
    {
        return new List<LoadAction>()
        {
            new LoadAction("Terrain", () =>
            {
                this.GenerateTerrain();
                this.Content.OnReloadCallback(new ContentId(@"Shaders\World\NoiseShader.hlsl", "Kernel") , _ => this.GenerateTerrain());
            }),
            new LoadAction("Lighting", () =>
            {
                var sun = this.Entities.Create();
                this.Components.Add(new SunLightComponent(sun, Color4.White, 3.0f));
                this.Components.Add(new CascadedShadowMapComponent(sun, this.Device, 2048, Cascades[0], Cascades[1], Cascades[2], Cascades[3]));
                this.Components.Add(new TransformComponent(sun)
                    .MoveTo(Vector3.UnitY)
                    .FaceTargetConstrained((-Vector3.UnitX * 0.75f) + (Vector3.UnitZ * 0.1f), Vector3.UnitY));
            }),
            new LoadAction("Skybox", () =>
            {
                var sky = this.Entities.Create();
                var texture = this.Content.LoadTexture(@"Skyboxes\circus.hdr");
                var albedo = this.CubeMapGenerator.GenerateAlbedo(texture, false, "skybox_albedo");
                var irradiance = this.CubeMapGenerator.GenerateIrradiance(texture, "skybox_irradiance");
                var environment = this.CubeMapGenerator.GenerateEnvironment(texture, "skybox_environment");

                // Make sure the items are disposed whenever this content frame is
                this.Content.Link(albedo, albedo.Name);
                this.Content.Link(irradiance, irradiance.Name);
                this.Content.Link(environment, environment.Name);

                this.Components.Add(new SkyboxComponent(sky, albedo, irradiance, environment, 0.1f));
            })
        };
    }

    private void GenerateTerrain()
    {
        if (this.world.HasValue)
        {
            this.Components.MarkForRemoval(this.world.Value);
        }

        var world = this.Entities.Create();

        var defaultMaterial = this.Content.LoadDefaultMaterial();
        var dimensions = 2;
        var heightMap = this.NoiseGenerator.Generate(dimensions);
        var model = this.Triangulator.Triangulate(this.Device, heightMap, dimensions, defaultMaterial, "terrain");
        this.Content.Link(model, "terrain");
        this.Components.Add(new ModelComponent(world, model));
        this.Components.Add(new TransformComponent(world));

        this.world = world;
    }
}
