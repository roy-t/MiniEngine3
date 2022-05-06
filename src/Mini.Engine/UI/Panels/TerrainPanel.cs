using System.Numerics;
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

    private int dimensions = 512;
    private Vector2 offset = Vector2.Zero;
    private float amplitude = 0.15f;
    private float frequency = 1.5f;
    private int octaves = 10;
    private float lacunarity = 1.0f;
    private float persistance = 0.55f;

    private readonly HydraulicErosionBrushSettings erosionSettings;

    private Entity? world;
    private TerrainComponent? terrain;

    public TerrainPanel(TerrainGenerator generator, UITextureRegistry registry, ContentManager content, ECSAdministrator administrator)
    {
        this.Generator = generator;
        this.Selector = new TextureSelector(registry);
        this.Content = content;
        this.Administrator = administrator;
        this.dimensions = 512;

        this.erosionSettings = new HydraulicErosionBrushSettings();

        this.Content.OnReloadCallback(new ContentId(@"Shaders\World\HeightMap.hlsl", "NoiseMapKernel"), _ => this.Recreate(this.ApplyTerrain));
        this.Content.OnReloadCallback(new ContentId(@"Shaders\World\HydraulicErosion.hlsl", "Kernel"), _ => { this.Recreate(this.ApplyTerrain); this.Recreate(this.ErodeTerrain); });
    }

    public string Title => "Terrain";

    public void Update(float elapsed)
    {
        var terrainChanged =
            ImGui.DragFloat2("Offset", ref this.offset, 0.1f) ||
            ImGui.SliderInt("Dimensions", ref this.dimensions, 4, 4096) ||
            ImGui.SliderInt("Octaves", ref this.octaves, 1, 20) ||
            ImGui.SliderFloat("Amplitude", ref this.amplitude, 0.01f, 2.0f) ||
            ImGui.SliderFloat("Persistance", ref this.persistance, 0.1f, 1.0f) ||
            ImGui.SliderFloat("Frequency", ref this.frequency, 0.1f, 10.0f) ||
            ImGui.SliderFloat("Lacunarity", ref this.lacunarity, 0.1f, 10.0f) ||

            // TODO: add cliff generation properties

            ImGui.Button("Generate");

        if (terrainChanged)
        {
            this.Recreate(this.ApplyTerrain);            
        }

        var erosionChanged =
            ImGui.SliderInt("Droplets", ref this.erosionSettings.Droplets, 1, 1_000_000) ||
            ImGui.SliderInt("DropletStride", ref this.erosionSettings.DropletStride, 1, 15) ||
            ImGui.SliderFloat("SedimentFactor", ref this.erosionSettings.SedimentFactor, 0.01f, 5.0f) ||
            ImGui.SliderFloat("MinSedimentCapacity", ref this.erosionSettings.MinSedimentCapacity, 0.0f, 0.001f) ||
            ImGui.SliderFloat("DepositSpeed", ref this.erosionSettings.DepositSpeed, 0.005f, 0.05f) ||
            ImGui.SliderFloat("MinSpeed", ref this.erosionSettings.MinSpeed, 0.0025f, 1.0f) ||
            ImGui.SliderFloat("MaxSpeed", ref this.erosionSettings.MaxSpeed, 1, 10.0f) ||
            ImGui.SliderFloat("Inertia", ref this.erosionSettings.Inertia, 0.0f, 0.99f) ||
            ImGui.SliderFloat("Gravity", ref this.erosionSettings.Gravity, 1.0f, 4.0f) ||
            ImGui.Button("Erode");

        if (erosionChanged)
        {
            this.Recreate(this.ErodeTerrain);
        }

        if (this.terrain != null)
        {
            if (this.Selector.Begin("Terrain Resources", "heightmap"))
            {
                this.Selector.Select("Height", this.terrain.Height);
                this.Selector.Select("Normals", this.terrain.Normals);
                this.Selector.Select("Tint", this.terrain.Tint);
                this.Selector.End();
            }
            this.Selector.ShowSelected(this.terrain.Height, this.terrain.Normals, this.terrain.Tint);
        }
    }

    private TerrainComponent ApplyTerrain(Entity world)
    {
        return this.Generator.Generate(world, this.dimensions, this.offset, this.amplitude, this.frequency, this.octaves, this.lacunarity, this.persistance, "terrain");      
    }

    private TerrainComponent ErodeTerrain(Entity world)
    {
        if (this.terrain is not null)
        {
            return this.Generator.Erode(world, this.terrain, this.erosionSettings, "terrain");
        }

        throw new NotSupportedException("Cannot erode null terrain");
    }

    private void Recreate(Func<Entity, TerrainComponent> application)
    {
        if(this.world.HasValue)
        {
            this.Administrator.Components.MarkForRemoval(this.world.Value);
        }

        this.world = this.Administrator.Entities.Create();
        this.terrain = application(this.world.Value);

        this.Administrator.Components.Add(this.terrain);

        var width = this.terrain.Mesh.Bounds.Max.X - this.terrain.Mesh.Bounds.Min.X;
        var desiredWidth = 10.0f;
        var scale = desiredWidth / width;
        this.Administrator.Components.Add(new TransformComponent(this.world.Value).SetScale(scale));
    }
}