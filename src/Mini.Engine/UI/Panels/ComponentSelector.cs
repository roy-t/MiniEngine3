using ImGuiNET;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.UI.Panels;
internal sealed class ComponentSelector<T>
    where T : struct, IComponent
{

    private readonly IComponentContainer<T> Container;
    private readonly List<Entity> Entities;

    private int selected;

    public ComponentSelector(string title, IComponentContainer<T> container)
    {
        this.Title = title;
        this.Container = container;
        this.Entities = new List<Entity>();
    }

    public string Title { get; set; }

    public void Update()
    {
        this.Entities.Clear();

        var iterator = this.Container.IterateAll();
        while (iterator.MoveNext())
        {
            ref var component = ref iterator.Current;
            this.Entities.Add(component.Entity);
        }

        Entity? selectedEntity = null;
        this.selected = Math.Max(0, Math.Min(this.selected, this.Entities.Count - 1));

        if (this.Entities.Any())
        {
            selectedEntity = this.Entities[this.selected];
        }

        if (ImGui.BeginCombo(this.Title, selectedEntity?.ToString() ?? string.Empty))
        {
            for (var i = 0; i < this.Entities.Count; i++)
            {
                var entity = this.Entities[i];
                if (ImGui.Selectable(entity.ToString(), entity == selectedEntity))
                {
                    this.selected = i;
                }
            }

            ImGui.EndCombo();
        }
    }

    public bool HasComponent()
    {
        return this.selected < this.Entities.Count;
    }

    public ref T Get()
    {
        return ref this.Container[this.Entities[this.selected]];
    }
}
