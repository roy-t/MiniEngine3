using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;

namespace Mini.Engine.UI.Panels;
[Service]
internal class TerrainPanel : IEditorPanel
{
    private readonly ComponentSelector<TerrainComponent> ComponentSelector;
    private readonly ECSAdministrator Administrator;

    private readonly TerrainGenerator Generator;

    private HeightMapGeneratorSettings settings;
    private HydraulicErosionBrushSettings erosionSettings;

    private bool instantUpdate = true;
    private bool heightMapChanged;
    private bool erosionChanged;

    private bool isErodingRealTime = false;
    private TimeSpan elapsedRealTime = TimeSpan.Zero;
    private readonly TimeSpan ExpectedRealTime = TimeSpan.FromSeconds(5);

    public TerrainPanel(ContentManager content, ECSAdministrator administrator, IComponentContainer<TerrainComponent> container, TerrainGenerator generator)
    {
        this.ComponentSelector = new ComponentSelector<TerrainComponent>("Terrain", container);
        this.Administrator = administrator;
        this.Generator = generator;

        this.settings = new HeightMapGeneratorSettings();
        this.erosionSettings = new HydraulicErosionBrushSettings();

        content.AddReloadCallback(new ContentId(@"Shaders\World\HeightMap.hlsl", "NoiseMapKernel"), () => this.heightMapChanged = true);
        content.AddReloadCallback(new ContentId(@"Shaders\World\HydraulicErosion.hlsl", "Kernel"), () => this.erosionChanged = true);
    }

    public string Title => "Terrain";

    public void Update(float elapsed)
    {
        if (this.isErodingRealTime)
        {
            this.UpdateRealtimeErosion(elapsed);
            return;
        }

        if (ImGui.CollapsingHeader("Manage", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.SliderInt("Dimensions", ref this.settings.Dimensions, 4, 4096);
            ImGui.SliderFloat("Definition", ref this.settings.MeshDefinition, 0.1f, 1.0f);

            this.ComponentSelector.Update();

            if (ImGui.Button("Add"))
            {
                this.CreateTerrain();
            }

            if (this.ComponentSelector.HasComponent())
            {
                ImGui.SameLine();
                if (ImGui.Button("Remove"))
                {
                    ref var component = ref this.ComponentSelector.Get();
                    this.Administrator.Components.MarkForRemoval(component.Entity);
                }
            }
        }

        if (ImGui.CollapsingHeader("Heigth Map", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (this.ComponentSelector.HasComponent())
            {
                this.heightMapChanged |= this.ShowHeightMapSettings();

                if (this.ShowHeightMapColorSettings())
                {
                    ref var component = ref this.ComponentSelector.Get().Value;
                    this.UpdateColors(ref component);
                }

                if (this.heightMapChanged || ImGui.Button("Update Heigth Map"))
                {
                    ref var component = ref this.ComponentSelector.Get().Value;
                    this.SetElevation(ref component);
                    this.heightMapChanged = false;
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset Heightmap"))
                {
                    this.settings = new HeightMapGeneratorSettings()
                    {
                        Dimensions = this.settings.Dimensions,
                        MeshDefinition = this.settings.MeshDefinition
                    };
                    this.heightMapChanged = true;
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset Colors"))
                {
                    ref var component = ref this.ComponentSelector.Get().Value;
                    var settings = new HeightMapGeneratorSettings();
                    this.settings.ErosionColor = settings.ErosionColor;
                    this.settings.DepositionColor = settings.DepositionColor;                    
                    this.settings.ErosionColorMultiplier = settings.ErosionColorMultiplier;
                    this.UpdateColors(ref component);
                }
            }
        }

        if (ImGui.CollapsingHeader("Erosion", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (this.ComponentSelector.HasComponent())
            {
                ImGui.Checkbox("Instant Update", ref this.instantUpdate);

                this.erosionChanged |= this.ShowErosionSettings();

                if ((this.erosionChanged && this.instantUpdate) || ImGui.Button("Set Erosion"))
                {
                    ref var component = ref this.ComponentSelector.Get().Value;
                    this.SetErosion(ref component);
                    this.erosionChanged = false;
                }

                ImGui.SameLine();
                if (ImGui.Button("Realtime Erosion"))
                {
                    ref var component = ref this.ComponentSelector.Get().Value;
                    this.isErodingRealTime = true;
                    this.elapsedRealTime = TimeSpan.Zero;
                }

                ImGui.SameLine();
                if (ImGui.Button("Iterate Erosion"))
                {
                    ref var component = ref this.ComponentSelector.Get().Value;
                    this.IterateErosion(ref component, this.erosionSettings);
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset Erosion"))
                {
                    this.erosionSettings = new HydraulicErosionBrushSettings();
                    this.erosionChanged = true;
                }
            }
        }
    }

    private void UpdateColors(ref TerrainComponent component)
    {
        component.ErosionColor = this.settings.ErosionColor;
        component.DepositionColor = this.settings.DepositionColor;
        component.ErosionColorMultiplier = this.settings.ErosionColorMultiplier;
    }

    private void UpdateRealtimeErosion(float elapsed)
    {
        ref var terrain = ref this.ComponentSelector.Get().Value;

        this.elapsedRealTime += TimeSpan.FromSeconds(elapsed);
        var perSecond = this.erosionSettings.Droplets / this.ExpectedRealTime.TotalSeconds;
        var step = elapsed * perSecond;
        var droplets = Math.Max(1, (int)step);

        var iterationSettings = this.erosionSettings.Copy();
        iterationSettings.Droplets = droplets;
        iterationSettings.Seed = Random.Shared.Next();

        this.IterateErosion(ref terrain, iterationSettings);

        if (this.elapsedRealTime >= this.ExpectedRealTime)
        {
            this.isErodingRealTime = false;
        }
    }

    private void SetElevation(ref TerrainComponent terrain)
    {
        this.Generator.UpdateElevation(ref terrain, this.settings);
    }

    private void SetErosion(ref TerrainComponent terrain)
    {
        this.Generator.UpdateElevation(ref terrain, this.settings);
        this.Generator.UpdateErosion(ref terrain, this.settings.MeshDefinition, this.erosionSettings);
    }

    private void IterateErosion(ref TerrainComponent terrain, HydraulicErosionBrushSettings iterationSettings)
    {
        this.Generator.UpdateErosion(ref terrain, this.settings.MeshDefinition, iterationSettings);
    }

    private void CreateTerrain()
    {
        var entity = this.Administrator.Entities.Create();
        ref var transform = ref this.Administrator.Components.Create<TransformComponent>(entity);
        transform.Current = transform.Current.SetScale(100.0f);

        ref var terrain = ref this.Administrator.Components.Create<TerrainComponent>(entity);
        this.UpdateColors(ref terrain);
        
        this.Generator.GenerateEmpty(ref terrain, this.settings.Dimensions, this.settings.MeshDefinition);
    }

    private bool ShowHeightMapSettings()
    {
        return ImGui.DragFloat2("Offset", ref this.settings.Offset, 0.1f) ||
               ImGui.SliderInt("Octaves", ref this.settings.Octaves, 1, 20) ||
               ImGui.SliderFloat("Amplitude", ref this.settings.Amplitude, 0.0f, 0.2f) ||
               ImGui.SliderFloat("Persistance", ref this.settings.Persistance, 0.25f, 0.75f) ||
               ImGui.SliderFloat("Frequency", ref this.settings.Frequency, 1.0f, 2.0f) ||
               ImGui.SliderFloat("Lacunarity", ref this.settings.Lacunarity, 0.75f, 1.25f) ||
               ImGui.SliderFloat("CliffStart", ref this.settings.CliffStart, 0.0f, 1.0f) ||
               ImGui.SliderFloat("CliffEnd", ref this.settings.CliffEnd, 0.0f, 1.0f) ||
               ImGui.SliderFloat("CliffStrength", ref this.settings.CliffStrength, 0.0f, 1.0f);
    }

    private bool ShowHeightMapColorSettings()
    {
        return ImGui.ColorEdit3("DepositionColor", ref this.settings.DepositionColor) ||
               ImGui.ColorEdit3("ErosionColor", ref this.settings.ErosionColor) ||
               ImGui.SliderFloat("ErosionColorMultiplier", ref this.settings.ErosionColorMultiplier, 1.0f, 1000.0f);
    }

    private bool ShowErosionSettings()
    {
        var changed = ImGui.SliderInt("Seed", ref this.erosionSettings.Seed, 0, int.MaxValue);
        ImGui.SameLine();
        if (ImGui.Button("randomize"))
        {
            this.erosionSettings.Seed = Random.Shared.Next();
            changed = true;
        }

        changed |=

               ImGui.SliderInt("Droplets", ref this.erosionSettings.Droplets, 1, 2_500_000) ||
               ImGui.SliderInt("DropletStride", ref this.erosionSettings.DropletStride, 1, 15) ||
               ImGui.SliderFloat("SedimentFactor", ref this.erosionSettings.SedimentFactor, 0.01f, 5.0f) ||
               ImGui.SliderFloat("MinSedimentCapacity", ref this.erosionSettings.MinSedimentCapacity, 0.0f, 0.001f) ||
               ImGui.SliderFloat("DepositSpeed", ref this.erosionSettings.DepositSpeed, 0.005f, 0.05f) ||
               ImGui.SliderFloat("Inertia", ref this.erosionSettings.Inertia, 0.0f, 0.99f) ||
               ImGui.SliderFloat("Gravity", ref this.erosionSettings.Gravity, 1.0f, 4.0f);

        return changed;
    }
}
