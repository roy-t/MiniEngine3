using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.Vegetation;
using Vortice.Mathematics;

namespace Mini.Engine.Scenes;

[Service]
public sealed class EmptyScene : IScene
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
    private readonly ECSAdministrator Administrator;
    private readonly CubeMapGenerator CubeMapGenerator;

    public EmptyScene(Device device, ContentManager content, ECSAdministrator administrator, CubeMapGenerator cubeMapGenerator)
    {
        this.Device = device;
        this.Content = content;
        this.Administrator = administrator;
        this.CubeMapGenerator = cubeMapGenerator;
    }

    public string Title => "Empty";

    public IReadOnlyList<LoadAction> Load()
    {
        var creator = this.Administrator.Components;

        return new List<LoadAction>()
        {
            new LoadAction("Lighting", () =>
            {
                var sun = this.Administrator.Entities.Create();

                ref var sunLight = ref creator.Create<SunLightComponent>(sun);
                sunLight.Color = Colors.White;
                sunLight.Strength = 3.0f;

                ref var shadowMap = ref creator.Create<CascadedShadowMapComponent>(sun);

                var resolution = 2048;
                var buffer = new DepthStencilBuffer(this.Device, "SunLight", DepthStencilFormat.D32_Float, resolution, resolution, 4);
                var bufferResource = this.Device.Resources.Add(buffer);
                this.Content.Link(bufferResource, buffer.Name);
                shadowMap.Init(bufferResource, resolution, Cascades[0], Cascades[1], Cascades[2], Cascades[3]);

                ref var transform = ref creator.Create<TransformComponent>(sun);
                transform.Transform = transform.Transform
                    .SetTranslation(Vector3.UnitY)
                    .FaceTargetConstrained((-Vector3.UnitX * 0.75f) + (Vector3.UnitZ * 0.1f), Vector3.UnitY);
            }),
            new LoadAction("Skybox", () =>
            {
                var sky = this.Administrator.Entities.Create();
                var texture = this.Content.LoadTexture(@"Skyboxes\industrial.hdr", string.Empty, TextureLoaderSettings.RenderData);
                var albedo = this.CubeMapGenerator.GenerateAlbedo(texture, sky.ToString());
                var irradiance = this.CubeMapGenerator.GenerateIrradiance(texture, sky.ToString());
                var environment = this.CubeMapGenerator.GenerateEnvironment(texture, sky.ToString());

                // Make sure the items are disposed whenever this content frame is
                this.Content.Link(albedo, nameof(albedo));
                this.Content.Link(irradiance, nameof(irradiance));
                this.Content.Link(environment, nameof(environment));

                var levels = this.Device.Resources.Get(environment).MipMapLevels;

                ref var skybox = ref creator.Create<SkyboxComponent>(sky);
                skybox.Init(albedo, irradiance, environment, levels, 0.1f);
            }),
            new LoadAction("Terrain", () =>
            {
                var grass = this.Administrator.Entities.Create();
                ref var grassy = ref creator.Create<GrassComponent>(grass);

                var instanceBuffer = new StructuredBuffer<GrassInstanceData>(this.Device, "Grass");
                instanceBuffer.MapData(this.Device.ImmediateContext, new GrassInstanceData[]
                {
                    new GrassInstanceData() { Position = Vector3.Zero},
                    //new GrassInstanceData() { Position = Vector3.UnitY * 5},
                    //new GrassInstanceData() { Position = Vector3.UnitY * 10},
                });

                var resource = this.Device.Resources.Add(instanceBuffer);
                this.Content.Link(resource, "Grass");
                grassy.InstanceBuffer = resource;
                grassy.Instances = 3;
            })
        };
    }   
}
