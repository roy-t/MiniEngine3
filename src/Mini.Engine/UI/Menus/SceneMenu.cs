using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Scenes;

namespace Mini.Engine.UI.Menus;

[Service]
internal sealed class SceneMenu : IMenu
{
    private readonly SceneManager SceneManager;    
    private int currentItem;
    

    public SceneMenu(SceneManager sceneManager)
    {
        this.SceneManager = sceneManager;        
    }

    public string Title => "Scenes";

    public void Update(float elapsed)
    {
        if (ImGui.BeginListBox("Scenes"))
        {
            for (var i = 0; i < this.SceneManager.Scenes.Count; i++)
            {
                var isSelected = i == this.currentItem;
                if (ImGui.Selectable(this.SceneManager.Scenes[i].Title, isSelected))
                {
                    this.currentItem = i;
                    this.SceneManager.Set(this.currentItem);
                }
            }
            
            ImGui.EndListBox();
        }
    }
}
