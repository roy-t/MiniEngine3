using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.Vegetation;
using Mini.Engine.Graphics.World;
using Mini.Engine.Graphics.World.Vegetation;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class GrassPanel : IPanel
{
    private readonly Device Device;
    private readonly GrassGenerator GrassPlacer;
    private readonly ContentManager Content;    
    private readonly ECSAdministrator Administrator;

    private readonly ComponentSelector<GrassComponent> GrassComponentSelector;
    private readonly ComponentSelector<TerrainComponent> TerrainComponentSelector;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public GrassPanel(Device device, GrassGenerator grassPlacer, ContentManager content, ECSAdministrator administrator, ContainerStore containerStore)
    {
        this.Device = device;
        this.GrassPlacer = grassPlacer;
        this.Content = content;
        this.Administrator = administrator;

        this.GrassComponentSelector = new ComponentSelector<GrassComponent>("Grass Component", containerStore.GetContainer<GrassComponent>());
        this.TerrainComponentSelector = new ComponentSelector<TerrainComponent>("Terrain Component", containerStore.GetContainer<TerrainComponent>());

        this.Transforms = containerStore.GetContainer<TransformComponent>();
    }

    public string Title => "Grass";

    public void Update(float elapsed)
    {
        ImGui.Text("Create");

        if (ImGui.Button("Single")) { this.CreateGrass(DebugGrassPlacer.DebugGrassLayout.Single); }
        ImGui.SameLine(); if (ImGui.Button("Line")) { this.CreateGrass(DebugGrassPlacer.DebugGrassLayout.Line); }
        ImGui.SameLine(); if (ImGui.Button("Random")) { this.CreateGrass(DebugGrassPlacer.DebugGrassLayout.Random); }
        ImGui.SameLine(); if (ImGui.Button("Clumped")) { this.CreateClumpedGrass(); }

        this.TerrainComponentSelector.Update();
        //if (this.TerrainComponentSelector.HasComponent())
        //{
        //    ImGui.SameLine();
        //    if (ImGui.Button("Create For")) { this.CreateClumpedGrass(); }
        //}

        ImGui.Separator();

        this.GrassComponentSelector.Update();                
        if (this.GrassComponentSelector.HasComponent() )
        {
            ImGui.SameLine();
            if (ImGui.Button("Remove"))
            {
                ref var component = ref this.GrassComponentSelector.Get();
                this.Administrator.Components.MarkForRemoval(component.Entity);
            }
        }

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




            if (this.TerrainComponentSelector.HasComponent())
            {
                ref var terrainComponent = ref this.TerrainComponentSelector.Get();
                if (ImGui.Button("Fit to terrain"))
                {
                    ref var transform = ref this.Transforms[terrainComponent.Entity];
                    grassComponent.InstanceBuffer = this.GrassPlacer.GenerateInstanceData(ref terrainComponent, ref transform, out var instances);
                    grassComponent.Instances = instances;
                }
            }
        }
    }

    private void CreateGrass(DebugGrassPlacer.DebugGrassLayout layout)
    {
        //var entity = this.Administrator.Entities.Create();
        //ref var component = ref this.Administrator.Components.Create<GrassComponent>(entity);

        //component.Texture = this.Content.LoadTexture(@"Shaders/World/GrassTexture.png");
        //component.InstanceBuffer = this.GrassPlacer.GenerateDebugGrass(layout, out var count);
        //component.Instances = count;
    }

    private void CreateClumpedGrass()
    {
        var entity = this.Administrator.Entities.Create();
        ref var component = ref this.Administrator.Components.Create<GrassComponent>(entity);

        //ref var terrainComponent = ref this.TerrainComponentSelector.Get();
        //ref var terrainTransform = ref this.Transforms[terrainComponent.Entity];
        component.Texture = this.Content.LoadTexture(@"Shaders/World/GrassTexture.png", TextureSettings.Default);
        //component.Texture = this.Content.LoadTexture(@"Shaders/World/GrassTexture.png");
        //component.InstanceBuffer = this.GrassPlacer.GenerateClumpedInstanceData(ref terrainComponent, ref terrainTransform, out var instances);
        component.InstanceBuffer = this.GrassPlacer.GenerateClumpedInstanceData(out var instances);
        component.Instances = instances;
    }
}
