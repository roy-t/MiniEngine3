using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Models;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
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

    private readonly LifetimeManager LifetimeManager;
    private readonly Device Device;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;
    private readonly SkyboxManager SkyboxManager;

    public SponzaScene(LifetimeManager lifetimeManager, Device device, ContentManager content, ECSAdministrator administrator, SkyboxManager skyboxManager)
    {
        this.LifetimeManager = lifetimeManager;
        this.Device = device;
        this.Content = content;
        this.Administrator = administrator;
        this.SkyboxManager = skyboxManager;
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
                var sponza = this.Content.LoadModel(@"Scenes\sponza\sponza.obj", ModelSettings.Default);

                ref var model = ref creator.Create<ModelComponent>(world);
                model.Model = sponza;

                ref var transform = ref creator.Create<TransformComponent>(world);
                transform.Current = transform.Current
                    .SetScale(0.05f);

                ref var caster = ref creator.Create<ShadowCasterComponent>(world);
                caster.Importance = float.MaxValue;
            }),
            new LoadAction("Lighting", () =>
            {
                var sphere = this.Administrator.Entities.Create();

                ref var pointLight = ref creator.Create<PointLightComponent>(sphere);
                pointLight.Color = Vector4.One;
                pointLight.Strength = 100.0f;

                ref var pointLightTransform = ref creator.Create<TransformComponent>(sphere);
                pointLightTransform.Current = pointLightTransform.Current
                    .SetTranslation(new Vector3(0, 1, 0));

                var sun = this.Administrator.Entities.Create();

                ref var sunLight = ref creator.Create<SunLightComponent>(sun);
                sunLight.Color = Colors.White;
                sunLight.Strength = 3.0f;

                ref var shadowmap = ref creator.Create<CascadedShadowMapComponent>(sun);

                var resolution = 2048;
                var buffer = new DepthStencilBuffer(this.Device, "SunLight", DepthStencilFormat.D32_Float, resolution, resolution, 4);
                var bufferResource = this.LifetimeManager.Add(buffer);                

                shadowmap.Init(bufferResource, resolution, Cascades[0], Cascades[1], Cascades[2], Cascades[3]);

                ref var sunTransform = ref creator.Create<TransformComponent>(sun);
                sunTransform.Current = sunTransform.Current
                    .SetTranslation(Vector3.UnitY)
                    .FaceTargetConstrained((-Vector3.UnitX * 0.75f) + (Vector3.UnitZ * 0.1f), Vector3.UnitY);
            }),
            new LoadAction("Skybox", () =>
            {
                this.SkyboxManager.SetCircusSkybox();
            })
        };
    }
}
