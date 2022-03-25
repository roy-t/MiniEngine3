using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
using Mini.Engine.UI.Components;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class TerrainPanel : IPanel
{
    private readonly TerrainGenerator Generator;
    private readonly TextureSelector Selector;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;

    private int dimensions;
    private Entity? world;
    private TerrainComponent? terrain;

    public TerrainPanel(TerrainGenerator generator, UITextureRegistry registry, ContentManager content, ECSAdministrator administrator)
    {
        this.Generator = generator;
        this.Selector = new TextureSelector(registry);
        this.Content = content;
        this.Administrator = administrator;
        this.dimensions = 512;

        this.Content.OnReloadCallback(new ContentId(@"Shaders\Noise\NoiseShader.hlsl", "Kernel"), _ => this.GenerateTerrain());
    }

    public string Title => "Terrain";

    public void Update(float elapsed)
    {
        if (ImGui.Button("Generate"))
        {
            this.GenerateTerrain();
        }

        if (this.terrain != null && this.Selector.Begin("Terrain Resources", "heightmap", this.terrain.HeightMap))
        {
            this.Selector.Select("Height Map", this.terrain.HeightMap);

            this.Selector.End();
        }

        this.Selector.ShowSelected();
    }

    private void GenerateTerrain()
    {
        if (this.world.HasValue)
        {
            this.Administrator.Components.MarkForRemoval(this.world.Value);
        }

        var world = this.Administrator.Entities.Create();

        this.terrain = this.Generator.Generate(world, this.dimensions, "terrain");
        this.Administrator.Components.Add(new TerrainComponent(world, this.terrain.HeightMap, this.terrain.Mesh));
        this.Administrator.Components.Add(new TransformComponent(world).SetScale(0.02f));

        this.world = world;
    }
}