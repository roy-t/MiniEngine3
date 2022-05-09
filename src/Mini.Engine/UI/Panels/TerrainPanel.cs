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


    private readonly HeightMapGeneratorSettings MapSettings;
    private readonly HydraulicErosionBrushSettings ErosionSettings;

    private Entity? world;
    private TerrainComponent? terrain;
    private TransformComponent? transform;

    public TerrainPanel(TerrainGenerator generator, UITextureRegistry registry, ContentManager content, ECSAdministrator administrator)
    {
        this.Generator = generator;
        this.Selector = new TextureSelector(registry);
        this.Content = content;
        this.Administrator = administrator;

        this.MapSettings = new HeightMapGeneratorSettings();
        this.ErosionSettings = new HydraulicErosionBrushSettings();

        this.Content.OnReloadCallback(new ContentId(@"Shaders\World\HeightMap.hlsl", "NoiseMapKernel"), _ => this.Recreate(this.ApplyTerrain));
        this.Content.OnReloadCallback(new ContentId(@"Shaders\World\HydraulicErosion.hlsl", "Kernel"), _ => { this.Recreate(this.ApplyTerrain); this.Recreate(this.ErodeTerrain); });
    }

    public string Title => "Terrain";

    public void Update(float elapsed)
    {
        if (this.terrain == null)
        {
            ImGui.SliderInt("Dimensions", ref this.MapSettings.Dimensions, 4, 4096);                        
            if (ImGui.Button("Generate"))
            {
                this.Recreate(this.ApplyTerrain);
            }
        }       
        else
        {
            // TODO: improve UI!

            ImGui.Text("Terrain generator settings");

            var terrainChanged =
                ImGui.DragFloat2("Offset", ref this.MapSettings.Offset, 0.1f) ||
                ImGui.SliderInt("Octaves", ref this.MapSettings.Octaves, 1, 20) ||
                ImGui.SliderFloat("Amplitude", ref this.MapSettings.Amplitude, 0.01f, 2.0f) ||
                ImGui.SliderFloat("Persistance", ref this.MapSettings.Persistance, 0.1f, 1.0f) ||
                ImGui.SliderFloat("Frequency", ref this.MapSettings.Frequency, 0.1f, 10.0f) ||
                ImGui.SliderFloat("Lacunarity", ref this.MapSettings.Lacunarity, 0.1f, 10.0f);           

            // TODO: add cliff generation properties

            if (terrainChanged)
            {
                this.Recreate(this.ApplyTerrain);
            }

            ImGui.Text("Erosion brush settings");

            var erosionChanged =
                ImGui.SliderInt("Droplets", ref this.ErosionSettings.Droplets, 1, 1_000_000) ||
                ImGui.SliderInt("DropletStride", ref this.ErosionSettings.DropletStride, 1, 15) ||
                ImGui.SliderFloat("SedimentFactor", ref this.ErosionSettings.SedimentFactor, 0.01f, 5.0f) ||
                ImGui.SliderFloat("MinSedimentCapacity", ref this.ErosionSettings.MinSedimentCapacity, 0.0f, 0.001f) ||
                ImGui.SliderFloat("DepositSpeed", ref this.ErosionSettings.DepositSpeed, 0.005f, 0.05f) ||
                ImGui.SliderFloat("Inertia", ref this.ErosionSettings.Inertia, 0.0f, 0.99f) ||
                ImGui.SliderFloat("Gravity", ref this.ErosionSettings.Gravity, 1.0f, 4.0f) ||
                ImGui.Button("Erode");

            if (erosionChanged)
            {
                this.Recreate(this.ApplyTerrain);
                this.Recreate(this.ErodeTerrain);
            }

            if (this.Selector.Begin("Terrain Resources", "heightmap"))
            {
                this.Selector.Select("Height", this.terrain.Terrain.Height);
                this.Selector.Select("Normals", this.terrain.Terrain.Normals);
                this.Selector.Select("Tint", this.terrain.Terrain.Tint);
                this.Selector.End();
            }
            this.Selector.ShowSelected(this.terrain.Terrain.Height, this.terrain.Terrain.Normals, this.terrain.Terrain.Tint);
        }
    }

    private TerrainMesh ApplyTerrain(TerrainMesh? input)
    {
        if (input is not null)
        {
            this.Generator.Update(input, this.MapSettings, "terrain");
            return input;
        }
        return this.Generator.Generate(this.MapSettings, "terrain");    
    }

    private TerrainMesh ErodeTerrain(TerrainMesh? input)
    {
        if (input is not null)
        {
            this.Generator.Erode(input, this.ErosionSettings, "terrain");
            return input;
        }

        throw new NotSupportedException("Cannot erode null terrain");
    }

    private void Recreate(Func<TerrainMesh?, TerrainMesh> application)
    {
        if(this.terrain == null || this.transform == null || !this.world.HasValue)
        {            
            var mesh = application(null);
            this.world = this.Administrator.Entities.Create();
            this.terrain = new TerrainComponent(this.world.Value, mesh);
            this.transform = new TransformComponent(this.world.Value);

            this.Administrator.Components.Add(this.terrain, this.transform);
        }
        else
        {
            application(this.terrain.Terrain);            
        }
        
        var width = this.terrain.Terrain.Mesh.Bounds.Max.X - this.terrain.Terrain.Mesh.Bounds.Min.X;
        var desiredWidth = 10.0f;
        var scale = desiredWidth / width;

        this.transform.SetScale(scale);
    }
}