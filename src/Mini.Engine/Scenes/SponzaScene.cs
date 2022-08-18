using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;
using Vortice.Mathematics;

namespace Mini.Engine.Scenes;

[Service]
public sealed class SponzaScene : IScene
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

    public SponzaScene(Device device, ContentManager content, ECSAdministrator administrator, CubeMapGenerator cubeMapGenerator)
    {
        this.Device = device;
        this.Content = content;
        this.Administrator = administrator;
        this.CubeMapGenerator = cubeMapGenerator;
    }

    public string Title => "Sponza";

    public IReadOnlyList<LoadAction> Load()
    {
        var creator = this.Administrator.Components;

        return new List<LoadAction>()
        {
            new LoadAction("Models", () =>
            {
                var world = this.Administrator.Entities.Create();
                var sponza = this.Content.LoadSponza();                

                ref var model = ref creator.Create<ModelComponent>(world);
                model.Model = sponza;

                ref var transform = ref creator.Create<TransformComponent>(world);
                transform.Transform = transform.Transform
                    .SetScale(0.05f);
            }),
            new LoadAction("Lighting", () =>
            {
                var sphere = this.Administrator.Entities.Create();

                ref var pointLight = ref creator.Create<PointLightComponent>(sphere);
                pointLight.Color = Vector4.One;
                pointLight.Strength = 100.0f;

                ref var pointLightTransform = ref creator.Create<TransformComponent>(sphere);
                pointLightTransform.Transform = pointLightTransform.Transform
                    .SetTranslation(new Vector3(0, 1, 0));
                
                var sun = this.Administrator.Entities.Create();

                ref var sunLight = ref creator.Create<SunLightComponent>(sun);
                sunLight.Color = Colors.White;
                sunLight.Strength = 3.0f;

                ref var shadowmap = ref creator.Create<CascadedShadowMapComponent>(sun);

                var resolution = 2048;
                var buffer = new DepthStencilBuffer(this.Device, DepthStencilFormat.D32_Float, resolution, resolution, 4, sun.ToString() + nameof(CascadedShadowMapComponent));
                var bufferResource = this.Device.Resources.Add(buffer);
                this.Content.Link(bufferResource, buffer.Name);

                shadowmap.Init(bufferResource, resolution, Cascades[0], Cascades[1], Cascades[2], Cascades[3]);

                ref var sunTransform = ref creator.Create<TransformComponent>(sun);
                sunTransform.Transform = sunTransform.Transform
                    .SetTranslation(Vector3.UnitY)
                    .FaceTargetConstrained((-Vector3.UnitX * 0.75f) + (Vector3.UnitZ * 0.1f), Vector3.UnitY);
            }),
            new LoadAction("Skybox", () =>
            {
                var sky = this.Administrator.Entities.Create();
                var texture = this.Content.LoadTexture(@"Skyboxes\circus.hdr", string.Empty, TextureLoaderSettings.RenderData);
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
            })
        };
    }
}
