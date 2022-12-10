using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
using Mini.Engine.UI.Components;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class TerrainPanel : IPanel
{
    private readonly Device Device;
    private readonly TerrainGenerator Generator;
    private readonly TextureSelector Selector;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;


    private HeightMapGeneratorSettings mapSettings;
    private HydraulicErosionBrushSettings erosionSettings;

    private Entity world;

    public TerrainPanel(Device device, TerrainGenerator generator, UITextureRegistry registry, ContentManager content, ECSAdministrator administrator)
    {
        this.Device = device;
        this.Generator = generator;
        this.Selector = new TextureSelector(registry);
        this.Content = content;
        this.Administrator = administrator;

        this.mapSettings = new HeightMapGeneratorSettings();
        this.erosionSettings = new HydraulicErosionBrushSettings();

        this.Content.AddReloadCallback(new ContentId(@"Shaders\World\HeightMap.hlsl", "NoiseMapKernel"), () => this.Recreate(this.ApplyTerrain));
        this.Content.AddReloadCallback(new ContentId(@"Shaders\World\HydraulicErosion.hlsl", "Kernel"), () => { this.Recreate(this.ApplyTerrain); this.Recreate(this.ErodeTerrain); });
    }

    public string Title => "Terrain";

    public void Update(float elapsed)
    {
        var created = this.Administrator.Entities.Entities.Contains(this.world);

        if (created == false)
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

            ref var terrain = ref this.Administrator.Components.GetComponent<TerrainComponent>(this.world);
            var height = this.Device.Resources.Get(terrain.Height);
            var normals = this.Device.Resources.Get(terrain.Normals);
            var tint = this.Device.Resources.Get(terrain.Tint);
            if (this.Selector.Begin("Terrain Resources", "heightmap"))
            {
                this.Selector.Select("Height", height);
                this.Selector.Select("Normals", normals);
                this.Selector.Select("Tint", tint);
                this.Selector.End();
            }
            this.Selector.ShowSelected(height, normals, tint);
        }
    }

    private GeneratedTerrain ApplyTerrain(GeneratedTerrain? input)
    {
        if (input is not null)
        {
            this.Generator.Update(input, this.mapSettings, "terrain");
            return input;
        }
        return this.Generator.Generate(this.mapSettings, "terrain");
    }

    private GeneratedTerrain ErodeTerrain(GeneratedTerrain? input)
    {
        if (input is not null)
        {
            this.Generator.Erode(input, this.mapSettings, this.erosionSettings, "terrain");
            return input;
        }

        throw new NotSupportedException("Cannot erode null terrain");
    }

    private void Recreate(Func<GeneratedTerrain?, GeneratedTerrain> application)
    {
        var created = this.Administrator.Entities.Entities.Contains(this.world);
        if (created == false)
        {
            var generated = application(null);
            this.world = this.Administrator.Entities.Create();

            var creator = this.Administrator.Components;

            ref var terrain = ref creator.Create<TerrainComponent>(this.world);

            terrain.Height = this.Device.Resources.Add(generated.Height);
            terrain.Mesh = this.Device.Resources.Add(generated.Mesh);
            terrain.Normals = this.Device.Resources.Add(generated.Normals);
            terrain.Tint = this.Device.Resources.Add(generated.Tint);

            ref var transform = ref creator.Create<TransformComponent>(this.world);
        }
        else
        {
            ref var terrain = ref this.Administrator.Components.GetComponent<TerrainComponent>(this.world);

            var model = (Mesh)this.Device.Resources.Get(terrain.Mesh);
            var height = (IRWTexture)this.Device.Resources.Get(terrain.Height);
            var normals = (IRWTexture)this.Device.Resources.Get(terrain.Normals);
            var tint = (IRWTexture)this.Device.Resources.Get(terrain.Tint);

            application(new GeneratedTerrain(height, normals, tint, model));
        }

        ref var ter = ref this.Administrator.Components.GetComponent<TerrainComponent>(this.world);
        ref var tra = ref this.Administrator.Components.GetComponent<TransformComponent>(this.world);

        var mesh = this.Device.Resources.Get(ter.Mesh);

        var width = mesh.Bounds.Max.X - mesh.Bounds.Min.X;
        var desiredWidth = 100.0f;
        var scale = desiredWidth / width;

        tra.Current = tra.Current.SetScale(scale);
    }
}