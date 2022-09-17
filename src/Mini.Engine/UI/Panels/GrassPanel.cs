using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Vegetation;
using Mini.Engine.Graphics.World;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class GrassPanel : IPanel
{
    private readonly Device Device;
    private readonly GrassPlacer GrassPlacer;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;

    private readonly ComponentSelector<GrassComponent> GrassComponentSelector;
    private readonly ComponentSelector<TerrainComponent> TerrainComponentSelector;


    public GrassPanel(Device device, GrassPlacer grassPlacer, ContentManager content, ECSAdministrator administrator, ContainerStore containerStore)
    {
        this.Device = device;
        this.GrassPlacer = grassPlacer;
        this.Content = content;
        this.Administrator = administrator;

        this.GrassComponentSelector = new ComponentSelector<GrassComponent>("Grass Component", containerStore.GetContainer<GrassComponent>());
        this.TerrainComponentSelector = new ComponentSelector<TerrainComponent>("Terrain Component", containerStore.GetContainer<TerrainComponent>());
    }

    public string Title => "Grass";

    public void Update(float elapsed)
    {
        ImGui.Text("Create ");
        ImGui.SameLine(); if (ImGui.Button("Single")) { this.CreateGrass(GrassPlacer.DebugGrassLayout.Single); }
        ImGui.SameLine(); if (ImGui.Button("Line")) { this.CreateGrass(GrassPlacer.DebugGrassLayout.Line); }
        ImGui.SameLine(); if (ImGui.Button("Random")) { this.CreateGrass(GrassPlacer.DebugGrassLayout.Random); }

        this.GrassComponentSelector.Update();

        ImGui.SameLine();
        if (this.GrassComponentSelector.HasComponent() && ImGui.Button("Remove"))
        {
            ref var component = ref this.GrassComponentSelector.Get();
            this.Administrator.Components.MarkForRemoval(component.Entity);
        }

        ImGui.Separator();

        if (this.GrassComponentSelector.HasComponent())
        {
            ref var grassComponent = ref this.GrassComponentSelector.Get();

            if (ImGui.BeginTable("GrassComponentTable", 2))
            {
                ImGui.TableSetupColumn("Property");
                ImGui.TableSetupColumn("Value");
                ImGui.TableHeadersRow();

                ImGui.TableNextColumn();
                ImGui.Text("Instances");
                ImGui.TableNextColumn();
                ImGui.Text(grassComponent.Instances.ToString());

                ImGui.EndTable();
            }

            this.TerrainComponentSelector.Update();


            if (this.TerrainComponentSelector.HasComponent())
            {
                ref var terrainComponent = ref this.TerrainComponentSelector.Get();
                if (ImGui.Button("Fit to terrain"))
                {
                    grassComponent.InstanceBuffer = this.GrassPlacer.GenerateInstanceData(ref terrainComponent, grassComponent.Instances);
                }
            }
        }
    }

    private void CreateGrass(GrassPlacer.DebugGrassLayout layout)
    {
        var entity = this.Administrator.Entities.Create();
        ref var component = ref this.Administrator.Components.Create<GrassComponent>(entity);

        component.Texture = this.Content.LoadTexture(@"Shaders/World/GrassTexture.png");
        component.InstanceBuffer = GrassPlacer.GenerateDebugGrass(layout, out var count);
        component.Instances = count;
    }
}
