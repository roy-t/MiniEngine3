using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Scenes;

namespace Mini.Engine.UI.Menus;

[Service]
internal sealed class SceneMenu : IEditorMenu
{
    private readonly SkyboxManager SkyboxManager;
    private readonly string[] Selections;

    private readonly SceneManager SceneManager;
    private int currentItem;

    public SceneMenu(SceneManager sceneManager, SkyboxManager skyboxManager)
    {
        this.SceneManager = sceneManager;
        this.SkyboxManager = skyboxManager;
        this.Selections = new string[]
        {
            @"Skyboxes\circus.hdr",
            @"Skyboxes\hilly_terrain.hdr",
            @"Skyboxes\industrial.hdr",
            @"Skyboxes\testgrid.jpg",
        };
    }

    public string Title => "Scenes";

    public void Update(float elapsed)
    {
        if (ImGui.BeginMenu("Scene"))
        {
            for (var i = 0; i < this.SceneManager.Scenes.Count; i++)
            {
                var isEnabled = i != this.SceneManager.ActiveScene;
                if (ImGui.MenuItem(this.SceneManager.Scenes[i].Title, isEnabled))
                {
                    this.currentItem = i;
                    this.SceneManager.Set(this.currentItem);
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Skybox"))
        {
            foreach (var selection in this.Selections)
            {
                if (ImGui.MenuItem(selection))
                {
                    this.SkyboxManager.SetSkybox(selection);
                }
            }

            ImGui.EndMenu();
        }
    }
}
