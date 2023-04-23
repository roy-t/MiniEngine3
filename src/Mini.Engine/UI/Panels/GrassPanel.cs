using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Textures;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.Vegetation;
using Mini.Engine.Graphics.World;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class GrassPanel : IEditorPanel
{
    private readonly GrassGenerator GrassPlacer;
    private readonly ContentManager Content;    
    private readonly ECSAdministrator Administrator;

    private readonly ComponentSelector<GrassComponent> GrassComponentSelector;
    private readonly ComponentSelector<TerrainComponent> TerrainComponentSelector;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public GrassPanel(GrassGenerator grassPlacer, ContentManager content, ECSAdministrator administrator, ContainerStore containerStore)
    {
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
        ImGui.TextUnformatted("Create");      

        this.TerrainComponentSelector.Update();        

        if (this.TerrainComponentSelector.HasComponent())
        {
            ref var terrainComponent = ref this.TerrainComponentSelector.Get();
            if (ImGui.Button("Place grass on terrain"))
            {
                ref var transform = ref this.Transforms[terrainComponent.Entity].Value;

                this.CreateClumpedGrass(ref terrainComponent.Value, in transform);
            }
        }

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
            ref var grassComponent = ref this.GrassComponentSelector.Get().Value;

            if (ImGui.BeginTable("GrassComponentTable", 2))
            {
                ImGui.TableSetupColumn("Property");
                ImGui.TableSetupColumn("Value");
                ImGui.TableHeadersRow();

                ImGui.TableNextColumn();
                ImGui.TextUnformatted("Instances");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(grassComponent.Instances.ToString());

                ImGui.EndTable();
            }            
        }
    }   

    private void CreateClumpedGrass(ref TerrainComponent terrainComponent, in TransformComponent terrainTransform)
    {
        var entity = this.Administrator.Entities.Create();
        ref var component = ref this.Administrator.Components.Create<GrassComponent>(entity);

        component.Texture = this.Content.LoadTexture(@"Shaders/World/GrassTexture.png", TextureSettings.Default);
        component.InstanceBuffer = this.GrassPlacer.GenerateClumpedInstanceData(ref terrainComponent, in terrainTransform, out var instances);
        component.Instances = instances;
    }
}
