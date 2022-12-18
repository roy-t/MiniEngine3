using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;

namespace Mini.Engine.UI.Panels;
[Service]
internal class TerrainPanelV2 : IPanel
{
    private readonly ComponentSelector<TerrainComponent> ComponentSelector;
    private readonly ECSAdministrator Administrator;

    private readonly TerrainGenerator Generator;

    private HeightMapGeneratorSettings settings;

    public TerrainPanelV2(ECSAdministrator administrator, IComponentContainer<TerrainComponent> container, TerrainGenerator generator)
    {
        this.ComponentSelector = new ComponentSelector<TerrainComponent>("Terrain", container);
        this.Administrator = administrator;
        this.Generator = generator;

        this.settings = new HeightMapGeneratorSettings();
    }

    public string Title => "Terrain V2";

    public void Update(float elapsed)
    {
        this.ComponentSelector.Update();
        if (this.ComponentSelector.HasComponent())
        {
            ref var component = ref this.ComponentSelector.Get();

            if (ImGui.Button("Remove"))
            {
                this.Administrator.Components.MarkForRemoval(component.Entity);
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Create"))
        {
            var entity = this.Administrator.Entities.Create();
            ref var transform = ref this.Administrator.Components.Create<TransformComponent>(entity);
            transform.Current = transform.Current.SetScale(100.0f);

            ref var terrain = ref this.Administrator.Components.Create<TerrainComponent>(entity);

            var generated = this.Generator.GenerateEmpty(this.settings.Dimensions, this.settings.MeshDefinition);

            terrain.Height = generated.Height;
            terrain.Mesh = generated.Mesh;
            terrain.Normals = generated.Normals;
            terrain.Erosion = generated.Erosion;
            terrain.ErosionColor = this.settings.ErosionColor;
            terrain.DepositionColor = this.settings.DepositionColor;
            terrain.ErosionColorMultiplier = this.settings.ErosionColorMultiplier;
        }
    }
}
