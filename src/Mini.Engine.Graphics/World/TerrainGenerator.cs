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

    public GeneratedTerrain GenerateEmpty(int dimensions, float meshDefinition)
    {
        var height = this.HeightMapGenerator.GenerateEmtpyHeights(dimensions);
        var normals = this.HeightMapGenerator.GenerateEmptyNormals(dimensions);
        var tint = this.HeightMapGenerator.GenerateTint(dimensions);

        var min = new Vector3(-0.5f, -0.01f, -0.5f);
        var max = new Vector3(0.5f, 0.01f, 0.5f);
        var bounds = new BoundingBox(min, max);

        var mesh = this.GenerateMesh(height, meshDefinition, bounds, "terrain");

        return new GeneratedTerrain
        (
            this.LifetimeManager.Add(height),
            this.LifetimeManager.Add(normals),
            this.LifetimeManager.Add(tint),
            this.LifetimeManager.Add(mesh)
        );
    }

    public GeneratedTerrain Generate(HeightMapGeneratorSettings settings, string name)
    {
        var height = this.HeightMapGenerator.GenerateHeights(settings);
        var normals = this.HeightMapGenerator.GenerateNormals(height);
        var tint = this.HeightMapGenerator.GenerateTint(settings.Dimensions);

        var bounds = ComputeBounds(settings);
        var mesh = this.GenerateMesh(height, settings.MeshDefinition, bounds, name);

        return new GeneratedTerrain
        (
            this.LifetimeManager.Add(height),
            this.LifetimeManager.Add(normals),
            this.LifetimeManager.Add(tint),
            this.LifetimeManager.Add(mesh)
        );
    }

    public void Update(GeneratedTerrain input, HeightMapGeneratorSettings settings, string name)
    {
        var res = this.Device.Resources;
        var height = res.Get(input.Height);
        var normals = res.Get(input.Normals);
        var tint = res.Get(input.Erosion);
        var mesh = res.Get(input.Mesh);

        this.HeightMapGenerator.UpdateHeights(height, settings);
        this.HeightMapGenerator.UpdateNormals(height, normals);
        this.HeightMapGenerator.UpdateTint(tint);

        var bounds = ComputeBounds(settings);
        this.UpdateMesh(mesh, settings.MeshDefinition, height, bounds);
    }

    public void Erode(GeneratedTerrain input, HeightMapGeneratorSettings heigthMapSettings, HydraulicErosionBrushSettings erosionSettings, string name)
    {
        var res = this.Device.Resources;
        var height = res.Get(input.Height);
        var normals = res.Get(input.Normals);
        var tint = res.Get(input.Erosion);
        var mesh = res.Get(input.Mesh);

        this.ErosionBrush.Apply(height, tint, erosionSettings);
        this.HeightMapGenerator.UpdateNormals(height, normals);
        this.UpdateMesh(mesh, heigthMapSettings.MeshDefinition, height, mesh.Bounds);
    }

    private Mesh GenerateMesh(IRWTexture height, float definition, BoundingBox bounds, string name)
    {
        var vertices = this.HeightMapGenerator.GenerateVertices(height, (int)(height.DimX * definition), (int)(height.DimY * definition));
        var indices = this.HeightMapGenerator.GenerateIndices((int)(height.DimX * definition), (int)(height.DimY * definition));
        var mesh = new Mesh(this.Device, bounds, vertices, indices, name, "mesh");
        return mesh;
    }

    private void UpdateMesh(IMesh input, float definition, IRWTexture height, BoundingBox bounds)
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