using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class SunPanel : IEditorPanel
{
    private readonly IComponentContainer<SunLightComponent> SunContainer;
    private readonly IComponentContainer<TransformComponent> TransformContainer;

    private readonly ComponentSelector<SunLightComponent> ComponentSelector;

    public SunPanel(IComponentContainer<SunLightComponent> sunContainer, IComponentContainer<TransformComponent> transformContainer)
    {
        this.SunContainer = sunContainer;
        this.TransformContainer = transformContainer;

        this.ComponentSelector = new ComponentSelector<SunLightComponent>("Sun Component", sunContainer);
    }

    public string Title => "Sun";

    public void Update()
    {
        this.ComponentSelector.Update();
        if (this.ComponentSelector.HasComponent())
        {
            ref var sun = ref this.ComponentSelector.Get();
            ref var transform = ref this.TransformContainer[sun.Entity].Value;

            var heigth = transform.Current.GetPosition().Y;
            var lightToSurface = transform.Current.GetForward();
            var target = MathF.Atan2(-lightToSurface.Z, lightToSurface.X);
            
            var directionChanged = ImGui.SliderFloat("Heigth", ref heigth, 0.0f, 1.0f)
             || ImGui.SliderFloat("Target", ref target, -MathF.PI, MathF.PI - 0.001f);

            if(directionChanged)
            {
                var foo = new Vector3(MathF.Cos(target), 0, -MathF.Sin(target));
                transform.Current = new Transform().SetTranslation(Vector3.UnitY * heigth)
                    .FaceTargetConstrained(foo, Vector3.UnitY);
            }

            var color = sun.Value.Color.ToVector4();
            if(ImGui.ColorEdit4("Color", ref color))
            {
                sun.Value.Color = new Color4(color);
            }
        }
    }
}
