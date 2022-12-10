using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainGenerator
{
    private readonly Device Device;
    private readonly HeightMapGenerator HeightMapGenerator;
    private readonly HydraulicErosionBrush ErosionBrush;
    private readonly LifetimeManager LifetimeManager;
   
    public TerrainGenerator(Device device, LifetimeManager content, HeightMapGenerator noiseGenerator, HydraulicErosionBrush erosionBrush)
    {
        this.Device = device;
        this.HeightMapGenerator = noiseGenerator;
        this.LifetimeManager = content;
        this.ErosionBrush = erosionBrush;
    }

    public GeneratedTerrain Generate(HeightMapGeneratorSettings settings, string name)
    {
        var height = this.HeightMapGenerator.GenerateHeights(settings);
        var normals = this.HeightMapGenerator.GenerateNormals(height);
        var tint = this.HeightMapGenerator.GenerateTint(settings.Dimensions);

        var bounds = ComputeBounds(settings);
        var mesh = this.GenerateMesh(height, settings.MeshDefinition, bounds, name);

        this.LifetimeManager.Add(height);
        this.LifetimeManager.Add(normals);
        this.LifetimeManager.Add(tint);
        this.LifetimeManager.Add(mesh);

        return new GeneratedTerrain(height, normals, tint, mesh);
    }

    public void Update(GeneratedTerrain input, HeightMapGeneratorSettings settings, string name)
    {
        this.HeightMapGenerator.UpdateHeights(input.Height, settings);
        this.HeightMapGenerator.UpdateNormals(input.Height, input.Normals);
        this.HeightMapGenerator.UpdateTint(input.Erosion);

        var bounds = ComputeBounds(settings);
        this.UpdateMesh(input.Mesh, settings.MeshDefinition, input.Height, bounds);
    }

    public void Erode(GeneratedTerrain terrain, HeightMapGeneratorSettings heigthMapSettings, HydraulicErosionBrushSettings erosionSettings, string name)
    {
        this.ErosionBrush.Apply(terrain.Height, terrain.Erosion, erosionSettings);
        this.HeightMapGenerator.UpdateNormals(terrain.Height, terrain.Normals);
        this.UpdateMesh(terrain.Mesh, heigthMapSettings.MeshDefinition,  terrain.Height, terrain.Mesh.Bounds);
    }

    private Mesh GenerateMesh(IRWTexture height, float definition, BoundingBox bounds, string name)
    {
        var vertices = this.HeightMapGenerator.GenerateVertices(height, (int)(height.DimX * definition), (int)(height.DimY * definition));
        var indices = this.HeightMapGenerator.GenerateIndices((int)(height.DimX * definition), (int)(height.DimY * definition));
        var mesh = new Mesh(this.Device, bounds, vertices, indices, name, "mesh");
        return mesh;
    }

    private void UpdateMesh(Mesh input, float definition, IRWTexture height, BoundingBox bounds)
    {        
        var vertices = this.HeightMapGenerator.GenerateVertices(height, (int)(height.DimX * definition), (int)(height.DimY * definition));
        input.Vertices.MapData(this.Device.ImmediateContext, vertices);

        var indices = this.HeightMapGenerator.GenerateIndices((int)(height.DimX * definition), (int)(height.DimY * definition));
        input.Indices.MapData(this.Device.ImmediateContext, indices);

        input.Bounds = bounds;
    }

    private static BoundingBox ComputeBounds(HeightMapGeneratorSettings settings)
    {
        var octaves = settings.Octaves;
        var amplitude = settings.Amplitude;
        var persistance = settings.Persistance;

        var cliffStrength = settings.CliffStrength;
        var cliffStart = settings.CliffStart;
        var cliffEnd = settings.CliffEnd;

        var weight = 0.0f;
        var a = amplitude;
        for (var i = 0; i < octaves; i++)
        {
            var noise = 1.0f * a;
            a *= persistance;
            weight += noise;
        }

        var heigth = weight + (amplitude * cliffStrength) * SmoothStep(amplitude * cliffStart, amplitude * cliffEnd, weight);

        var min = new Vector3(-0.5f, -0.5f * weight, -0.5f);
        var max = new Vector3(0.5f, 0.5f * heigth, 0.5f);
        return new BoundingBox(min, max);
    }

    private static float SmoothStep(float from, float to, float weight)
    {
        var x = Math.Clamp((weight - from) / (to - from), 0.0f, 1.0f);
        return x * x * (3.0f - (2.0f * x));
    }

}