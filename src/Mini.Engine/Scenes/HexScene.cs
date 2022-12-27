using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Hexagons;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;
using Vortice.Mathematics;

using HexagonInstanceData = Mini.Engine.Content.Shaders.Generated.Hexagon.InstanceData;

namespace Mini.Engine.Scenes;

[Service]
public sealed class HexScene : IScene
{
    private static readonly float[] Cascades =
{
        0.075f,
        0.15f,
        0.3f,
        1.0f
    };

    private readonly Device Device;
    private readonly LifetimeManager LifetimeManager;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;
    private readonly CubeMapGenerator CubeMapGenerator;

    public HexScene(Device device, LifetimeManager lifetimeManager, ContentManager content, ECSAdministrator administrator, CubeMapGenerator cubeMapGenerator)
    {
        this.Device = device;
        this.LifetimeManager = lifetimeManager;
        this.Content = content;
        this.Administrator = administrator;
        this.CubeMapGenerator = cubeMapGenerator;
    }

    public string Title => "Hex";

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
                var bufferResource = this.LifetimeManager.Add(buffer);
                shadowMap.Init(bufferResource, resolution, Cascades[0], Cascades[1], Cascades[2], Cascades[3]);

                ref var transform = ref creator.Create<TransformComponent>(sun);
                transform.Current = transform.Current
                    .SetTranslation(Vector3.UnitY)
                    .FaceTargetConstrained((-Vector3.UnitX * 0.75f) + (Vector3.UnitZ * 0.1f), Vector3.UnitY);
            }),
            new LoadAction("Skybox", () =>
            {
                var sky = this.Administrator.Entities.Create();
                var texture = this.Content.LoadTexture(@"Skyboxes\hilly_terrain.hdr", TextureSettings.RenderData);
                var albedo = this.CubeMapGenerator.GenerateAlbedo(texture, sky.ToString());
                var irradiance = this.CubeMapGenerator.GenerateIrradiance(texture, sky.ToString());
                var environment = this.CubeMapGenerator.GenerateEnvironment(texture, sky.ToString());

                var levels = this.Device.Resources.Get(environment).MipMapLevels;

                ref var skybox = ref creator.Create<SkyboxComponent>(sky);
                skybox.Init(albedo, irradiance, environment, levels, 0.1f);
            }),
            new LoadAction("Terrain", () =>
            {
                var entity = this.Administrator.Entities.Create();

                ref var transform = ref creator.Create<TransformComponent>(entity);
                transform.Previous = Transform.Identity;
                transform.Current = Transform.Identity;

                ref var hexes = ref creator.Create<HexagonTerrainComponent>(entity);
                hexes.Material = this.Content.LoadMaterial(new ContentId(@"Materials\Grass01_MR_2K\grass.mtl", "grass"), MaterialSettings.Default);                

                var data = new HexagonInstanceData[]
                {
                    new HexagonInstanceData()
                    {
                        Position = Vector3.Zero,
                        Sides = 0u
                    }
                };
                var instanceBuffer = new StructuredBuffer<HexagonInstanceData>(this.Device, "Hexagons");
                instanceBuffer.MapData(this.Device.ImmediateContext, data);
       
                hexes.InstanceBuffer = this.Device.Resources.Add(instanceBuffer);
                hexes.Instances = data.Length;
            })
        };
    }
}
