﻿using System.Numerics;
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
    private float amplitude = 0.30f;
    private float frequency = 1.0f;
    private int octaves = 7;
    private float lacunarity = 2.25f;
    private float persistance = 0.35f;

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
        var changed =
            ImGui.SliderInt("Dimensions", ref this.dimensions, 4, 4096) ||
            ImGui.DragFloat2("Offset", ref this.offset, 0.1f) ||
            ImGui.SliderFloat("Amplitude", ref this.amplitude, 0.01f, 2.0f) ||
            ImGui.SliderFloat("Frequency", ref this.frequency, 0.1f, 10.0f) ||
            ImGui.SliderInt("Octaves", ref this.octaves, 1, 10) ||
            ImGui.SliderFloat("Lacunarity", ref this.lacunarity, 1.0f, 10.0f) ||
            ImGui.SliderFloat("Persistance", ref this.persistance, 0.1f, 1.0f) ||
            ImGui.Button("Generate");

        if (changed)
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

        this.terrain = this.Generator.Generate(world, this.dimensions, this.offset, this.amplitude, this.frequency, this.octaves, this.lacunarity, this.persistance, "terrain");
        this.Administrator.Components.Add(new TerrainComponent(world, this.terrain.HeightMap, this.terrain.Mesh));

        var width = this.terrain.Mesh.Bounds.Maximum.X - this.terrain.Mesh.Bounds.Minimum.X;
        var desiredWidth = 10.0f;
        var scale = desiredWidth / width;        
        this.Administrator.Components.Add(new TransformComponent(world).SetScale(scale));

         this.world = world;
    }
}