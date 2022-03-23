using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.Graphics.Models;
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
    private readonly EntityAdministrator Entities;
    private readonly ComponentAdministrator Components;

    private int dimensions;
    private Entity? world;
    private Terrain? terrain;

    public TerrainPanel(TerrainGenerator generator, TextureSelector selector, ContentManager content, EntityAdministrator entities, ComponentAdministrator components)
    {
        this.Generator = generator;
        this.Selector = selector;
        this.Content = content;
        this.Entities = entities;
        this.Components = components;
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

        if (this.terrain != null && this.Selector.Begin("Terrain Resources"))
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
            this.Components.MarkForRemoval(this.world.Value);
        }

        var world = this.Entities.Create();

        this.terrain = this.Generator.Generate(this.dimensions, "terrain");
        this.Components.Add(new ModelComponent(world, this.terrain.Mesh));
        this.Components.Add(new TransformComponent(world).SetScale(0.02f));

        this.world = world;
    }
}