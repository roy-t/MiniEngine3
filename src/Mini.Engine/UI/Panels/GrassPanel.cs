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
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;

    private readonly ComponentSelector<GrassComponent> GrassComponentSelector;
    private readonly ComponentSelector<TerrainComponent> TerrainComponentSelector;


    public GrassPanel(Device device, ContentManager content, ECSAdministrator administrator, ContainerStore containerStore)
    {
        this.Device = device;
        this.Content = content;
        this.Administrator = administrator;

        this.GrassComponentSelector = new ComponentSelector<GrassComponent>("Grass Component", containerStore.GetContainer<GrassComponent>());
        this.TerrainComponentSelector = new ComponentSelector<TerrainComponent>("Terrain Component", containerStore.GetContainer<TerrainComponent>());
    }

    public string Title => "Grass";

    public void Update(float elapsed)
    {
        this.GrassComponentSelector.Update();

        ImGui.Separator();

        if (this.GrassComponentSelector.HasComponent())
        {
            ref var component = ref this.GrassComponentSelector.Get();

            if(ImGui.BeginTable("GrassComponentTable", 2))
            {
                ImGui.TableSetupColumn("Property");
                ImGui.TableSetupColumn("Value");
                ImGui.TableHeadersRow();

                ImGui.TableNextColumn();
                ImGui.Text("Instances");
                ImGui.TableNextColumn();
                ImGui.Text(component.Instances.ToString());

                ImGui.EndTable();
            }
        }

        this.TerrainComponentSelector.Update();
    }

    // TODO: create a compute shader that generates the position information for each grass blade
    // based on the selected terrain's 
}
