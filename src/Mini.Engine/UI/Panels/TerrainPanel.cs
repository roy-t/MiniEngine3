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


    private HeightMapGeneratorSettings mapSettings;
    private HydraulicErosionBrushSettings erosionSettings;

    private Entity? world;
    private TerrainComponent? terrain;
    private TransformComponent? transform;

    public TerrainPanel(TerrainGenerator generator, UITextureRegistry registry, ContentManager content, ECSAdministrator administrator)
    {
        this.Generator = generator;
        this.Selector = new TextureSelector(registry);
        this.Content = content;
        this.Administrator = administrator;

        this.mapSettings = new HeightMapGeneratorSettings();
        this.erosionSettings = new HydraulicErosionBrushSettings();

        this.Content.OnReloadCallback(new ContentId(@"Shaders\World\HeightMap.hlsl", "NoiseMapKernel"), _ => this.Recreate(this.ApplyTerrain));
        this.Content.OnReloadCallback(new ContentId(@"Shaders\World\HydraulicErosion.hlsl", "Kernel"), _ => { this.Recreate(this.ApplyTerrain); this.Recreate(this.ErodeTerrain); });
    }

    public string Title => "Terrain";

    public void Update(float elapsed)
    {
        // TODO: allow recreating the entire mesh
        if (this.terrain == null)
        {
            ImGui.SliderInt("Dimensions", ref this.mapSettings.Dimensions, 4, 4096);
            ImGui.SliderFloat("MeshDefinition", ref this.mapSettings.MeshDefinition, 0.1f, 1.0f);
            if (ImGui.Button("Generate"))
            {
                this.Recreate(this.ApplyTerrain);
            }
        }       
        else
        {
            ImGui.Text("Terrain generator settings");

            var terrainChanged =
                ImGui.DragFloat2("Offset", ref this.mapSettings.Offset, 0.1f) ||
                ImGui.SliderInt("Octaves", ref this.mapSettings.Octaves, 1, 20) ||
                ImGui.SliderFloat("Amplitude", ref this.mapSettings.Amplitude, 0.01f, 2.0f) ||
                ImGui.SliderFloat("Persistance", ref this.mapSettings.Persistance, 0.1f, 1.0f) ||
                ImGui.SliderFloat("Frequency", ref this.mapSettings.Frequency, 0.1f, 10.0f) ||
                ImGui.SliderFloat("Lacunarity", ref this.mapSettings.Lacunarity, 0.1f, 10.0f) ||
                ImGui.SliderFloat("CliffStart", ref this.mapSettings.CliffStart, 0.0f, 1.0f) ||
                ImGui.SliderFloat("CliffEnd", ref this.mapSettings.CliffEnd, 0.0f, 1.0f) ||
                ImGui.SliderFloat("CliffStrength", ref this.mapSettings.CliffStrength, 0.0f, 1.0f);

            if (ImGui.Button("Reset Height Map Generator Settings"))
            {
                this.mapSettings = new HeightMapGeneratorSettings();
                terrainChanged = true;
                
            }

            if (terrainChanged)
            {
                this.Recreate(this.ApplyTerrain);
            }

            ImGui.Text("Erosion brush settings");

            var erosionChanged =
                ImGui.SliderInt("Seed", ref this.erosionSettings.Seed, 0, int.MaxValue) ||
                ImGui.SliderInt("Droplets", ref this.erosionSettings.Droplets, 1, 1_000_000) ||
                ImGui.SliderInt("DropletStride", ref this.erosionSettings.DropletStride, 1, 15) ||
                ImGui.SliderFloat("SedimentFactor", ref this.erosionSettings.SedimentFactor, 0.01f, 5.0f) ||
                ImGui.SliderFloat("MinSedimentCapacity", ref this.erosionSettings.MinSedimentCapacity, 0.0f, 0.001f) ||
                ImGui.SliderFloat("DepositSpeed", ref this.erosionSettings.DepositSpeed, 0.005f, 0.05f) ||
                ImGui.SliderFloat("Inertia", ref this.erosionSettings.Inertia, 0.0f, 0.99f) ||
                ImGui.SliderFloat("Gravity", ref this.erosionSettings.Gravity, 1.0f, 4.0f) ||
                ImGui.SliderFloat("ErosionTintFactor", ref this.erosionSettings.ErosionTintFactor, -50.0f, 50.0f) ||
                ImGui.SliderFloat("BuildUpTintFactor", ref this.erosionSettings.BuildUpTintFactor, -50.0f, 50.0f);


            if (ImGui.Button("Randomize Seed"))
            {
                this.erosionSettings.Seed = Random.Shared.Next();
                erosionChanged = true;
            }

            if (ImGui.Button("Reset Hydraulic Erosion Brush Settings"))
            {
                this.erosionSettings = new HydraulicErosionBrushSettings();
                erosionChanged = true;

            }

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
            this.Generator.Update(input, this.mapSettings, "terrain");
            return input;
        }
        return this.Generator.Generate(this.mapSettings, "terrain");    
    }

    private TerrainMesh ErodeTerrain(TerrainMesh? input)
    {
        if (input is not null)
        {
            this.Generator.Erode(input, this.erosionSettings, "terrain");
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