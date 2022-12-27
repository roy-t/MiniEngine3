using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Materials;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Hexagons;
using Mini.Engine.Graphics.Transforms;

using HexagonInstanceData = Mini.Engine.Content.Shaders.Generated.Hexagon.InstanceData;

namespace Mini.Engine.UI.Panels;

[Service]
internal class HexagonPanel : IPanel
{
    private readonly ComponentSelector<HexagonTerrainComponent> ComponentSelector;
    private readonly Device Device;
    private readonly ECSAdministrator Administrator;
    private readonly ContentManager Content;

    private readonly GeneratorSettings Settings;

    public HexagonPanel(Device device, ECSAdministrator administrator, ContentManager content, IComponentContainer<HexagonTerrainComponent> container)
    {
        this.Device = device;
        this.Administrator = administrator;
        this.Content = content;

        this.ComponentSelector = new ComponentSelector<HexagonTerrainComponent>("Components", container);

        this.Settings = new GeneratorSettings();
    }

    public string Title => "Hexes";

    public void Update(float elapsed)
    {
        ImGui.SliderInt("Columns", ref this.Settings.Columns, 1, 1000);
        ImGui.SliderInt("Rows", ref this.Settings.Rows, 1, 1000);

        this.ComponentSelector.Update();
        if (ImGui.Button("Add"))
        {
            this.CreateHexes();
        }

        if (this.ComponentSelector.HasComponent())
        {
            ImGui.SameLine();
            if (ImGui.Button("Remove"))
            {
                ref var component = ref this.ComponentSelector.Get();
                this.Administrator.Components.MarkForRemoval(component.Entity);
            }
        }
    }

    private void CreateHexes()
    {
        var entity = this.Administrator.Entities.Create();
        var creator = this.Administrator.Components;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Previous = Transform.Identity;
        transform.Current = Transform.Identity;

        ref var hexes = ref creator.Create<HexagonTerrainComponent>(entity);
        hexes.Material = this.Content.LoadMaterial(new ContentId(@"Materials\Grass01_MR_2K\grass.mtl", "grass"), MaterialSettings.Default);

        var data = HexagonTerrainBuilder.Create(this.Settings.Columns, this.Settings.Rows);

        var instanceBuffer = new StructuredBuffer<HexagonInstanceData>(this.Device, "Hexagons");
        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        hexes.InstanceBuffer = this.Device.Resources.Add(instanceBuffer);
        hexes.Instances = data.Length;
    }

    internal sealed class GeneratorSettings
    {
        public int Columns;
        public int Rows;

        public GeneratorSettings(int columns = 10, int rows = 5)
        {
            this.Columns = columns;
            this.Rows = rows;
        }
    }
}
