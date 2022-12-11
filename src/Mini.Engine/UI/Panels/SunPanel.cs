using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class SunPanel : IPanel
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

    public void Update(float elapsed)
    {
        this.ComponentSelector.Update();
        if (this.ComponentSelector.HasComponent())
        {
            ref var sun = ref this.ComponentSelector.Get();
            ref var transform = ref this.TransformContainer[sun.Entity];

            var yawPitchRoll = transform.Current.GetRotation().ToEuler();
            var yaw = yawPitchRoll.X;
            var pitch = yawPitchRoll.Y;            

            var changed = ImGui.SliderFloat("Yaw", ref yaw, -MathF.PI, 0.0f) ||
                ImGui.SliderFloat("Pitch", ref pitch, -MathF.PI * 0.499f, MathF.PI * 0.499f);

            if (changed)
            {                
                var rotation = NumericsExtensions.FromEuler(new Vector3(yaw, pitch, 0.0f));
                transform.Current = new Transform(Vector3.Zero, rotation, Vector3.Zero, 1.0f);
            }
        }
    }
}
