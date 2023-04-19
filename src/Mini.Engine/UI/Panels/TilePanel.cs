using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Materials;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Tiles;
using Mini.Engine.Graphics.Transforms;
using Vortice.Mathematics;
using TileInstanceData = Mini.Engine.Content.Shaders.Generated.Tiles.InstanceData;

namespace Mini.Engine.UI.Panels;

[Service]
internal class TilePanel : IPanel
{
    private readonly ComponentSelector<TileComponent> TileComponentSelector;
    private readonly ComponentSelector<TileHighlightComponent> TileHighlightComponentSelector;
    private readonly Device Device;
    private readonly ECSAdministrator Administrator;
    private readonly ContentManager Content;

    private readonly GeneratorSettings Settings;

    private int minColumn;
    private int maxColumn;
    private int minRow;
    private int maxRow;

    public TilePanel(Device device, ECSAdministrator administrator, ContentManager content, IComponentContainer<TileComponent> tiles, IComponentContainer<TileHighlightComponent> highlights)
    {
        this.Device = device;
        this.Administrator = administrator;
        this.Content = content;

        this.TileComponentSelector = new ComponentSelector<TileComponent>("Tile Components", tiles);
        this.TileHighlightComponentSelector = new ComponentSelector<TileHighlightComponent>("Highlight Components", highlights);

        this.Settings = new GeneratorSettings();
    }

    public string Title => "Tiles";

    public void Update(float elapsed)
    {
        ImGui.SliderInt("Columns", ref this.Settings.Columns, 1, 2048);
        ImGui.SliderInt("Rows", ref this.Settings.Rows, 1, 2048);

        this.TileComponentSelector.Update();
        if (ImGui.Button("Add"))
        {
            this.CreateTiles();
        }

        if (this.TileComponentSelector.HasComponent())
        {
            ImGui.SameLine();
            ref var component = ref this.TileComponentSelector.Get();
            if (ImGui.Button("Remove"))
            {
                this.Administrator.Components.MarkForRemoval(component.Entity);
            }

            this.TileHighlightComponentSelector.Update();
            ImGui.DragIntRange2("Column Range", ref this.minColumn, ref this.maxColumn, 0.05f, 0, (int)(component.Value.Columns - 1));
            ImGui.DragIntRange2("Row Range", ref this.minRow, ref this.maxRow, 0.05f, 0, (int)(component.Value.Rows - 1));


            if (this.TileHighlightComponentSelector.HasComponent())
            {
                ref var highlight = ref this.TileHighlightComponentSelector.Get();
                if (ImGui.Button("Remove Highlight"))
                {
                    highlight.LifeCycle = highlight.LifeCycle.ToRemoved();
                }
            }
            else
            {
                if (ImGui.Button("Add Highlight"))
                {
                    this.CreateHighlight(component.Entity);
                }
            }
        }
    }

    private void CreateHighlight(Entity entity)
    {        
        ref var component = ref this.Administrator.Components.Create<TileHighlightComponent>(entity);
        component.MinColumn = (uint)this.minColumn;
        component.MaxColumn = (uint)this.maxColumn;
        component.MinRow = (uint)this.minRow;
        component.MaxRow = (uint)this.maxRow;
        component.Tint = Colors.Red;
    }

    private void CreateTiles()
    {
        var entity = this.Administrator.Entities.Create();
        var creator = this.Administrator.Components;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity.SetScale(1);
        transform.Previous = transform.Current;

        ref var tile = ref creator.Create<TileComponent>(entity);
        tile.TopMaterial = this.Content.LoadMaterial(new ContentId(@"Materials\Grass01_MR_2K\grass.mtl", "grass"), MaterialSettings.Default);
        tile.WallMaterial = this.Content.LoadMaterial(new ContentId(@"Materials\Rock05_MR_2K\rock.mtl", "rock"), MaterialSettings.Default);

        var data = TileBuilder.Create(this.Settings.Columns, this.Settings.Rows);

        var instanceBuffer = new StructuredBuffer<TileInstanceData>(this.Device, "Tiles");
        instanceBuffer.MapData(this.Device.ImmediateContext, data);

        tile.InstanceBuffer = this.Device.Resources.Add(instanceBuffer);
        tile.Columns = (uint)this.Settings.Columns;
        tile.Rows = (uint)this.Settings.Rows;
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
