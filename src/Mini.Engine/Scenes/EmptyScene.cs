using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;
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
    private readonly LifetimeManager LifetimeManager;
    private readonly ECSAdministrator Administrator;
    private readonly SkyboxManager SkyboxManager;

    public EmptyScene(Device device, LifetimeManager lifetimeManager, ECSAdministrator administrator, SkyboxManager skyboxManager)
    {
        this.Device = device;
        this.LifetimeManager = lifetimeManager;
        this.Administrator = administrator;
        this.SkyboxManager = skyboxManager;
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
                var bufferResource = this.LifetimeManager.Add(buffer);
                shadowMap.Init(bufferResource, resolution, Cascades[0], Cascades[1], Cascades[2], Cascades[3]);

                ref var transform = ref creator.Create<TransformComponent>(sun);
                transform.Current = transform.Current
                    .SetTranslation(Vector3.UnitY)
                    .FaceTargetConstrained((-Vector3.UnitX * 0.75f) + (Vector3.UnitZ * 0.1f), Vector3.UnitY);
            }),
            new LoadAction("Skybox", () =>
            {
                this.SkyboxManager.SetHillyTerrainSkybox();
            })
        };
    }
}
