using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Textures;
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
    private readonly ContentManager Content;
    private readonly Pixels Pixels;
    private readonly LifetimeManager LifetimeManager;

    public TerrainGenerator(Device device, ContentManager content, Pixels pixels, LifetimeManager lifetimeManager, HeightMapGenerator noiseGenerator, HydraulicErosionBrush erosionBrush)
    {
        this.Device = device;
        this.Content = content;
        this.Pixels = pixels;
        this.LifetimeManager = lifetimeManager;

        this.HeightMapGenerator = noiseGenerator;
        this.ErosionBrush = erosionBrush;
    }

    public void GenerateEmpty(ref TerrainComponent component, int dimensions, float meshDefinition)
    {
        var min = new Vector3(-0.5f, -0.01f, -0.5f);
        var max = new Vector3(0.5f, 0.01f, 0.5f);
        var bounds = new BoundingBox(min, max);

        var height = this.HeightMapGenerator.GenerateEmtpyHeights(dimensions);
        var mesh = this.GenerateMesh(height, meshDefinition, bounds, "terrain");
        var normals = this.HeightMapGenerator.GenerateEmptyNormals(dimensions);
        var erosion = this.HeightMapGenerator.GenerateTint(dimensions);

        component.Height = this.LifetimeManager.Add(height);
        component.Mesh = this.LifetimeManager.Add(mesh);
        component.Normals = this.LifetimeManager.Add(normals);
        component.Erosion = this.LifetimeManager.Add(erosion);

        component.Foilage = this.Pixels.CreatePixel(new Color4(0.0f, 0, 0, 1.0f), "Foilage");

        component.Material = this.Content.LoadMaterial(new ContentId(@"Materials\Grass01_MR_2K\grass.mtl", "grass"), MaterialSettings.Default);
    }

    public void UpdateElevation(ref TerrainComponent component, HeightMapGeneratorSettings settings)
    {
        var res = this.Device.Resources;
        var height = (IRWTexture)res.Get(component.Height);
        var normals = (IRWTexture)res.Get(component.Normals);
        var tint = (IRWTexture)res.Get(component.Erosion);
        var mesh = res.Get(component.Mesh);

        this.HeightMapGenerator.UpdateHeights(height, settings);
        this.HeightMapGenerator.UpdateNormals(height, normals);
        this.HeightMapGenerator.UpdateTint(tint);

        var bounds = ComputeBounds(settings);
        this.UpdateMesh(mesh, settings.MeshDefinition, height, bounds);
    }

    public void UpdateErosion(ref TerrainComponent component, float meshDefinition, HydraulicErosionBrushSettings settings)
    {
        var res = this.Device.Resources;
        var height = (IRWTexture)res.Get(component.Height);
        var normals = (IRWTexture)res.Get(component.Normals);
        var tint = (IRWTexture)res.Get(component.Erosion);
        var mesh = res.Get(component.Mesh);

        this.ErosionBrush.Apply(height, tint, settings);
        this.HeightMapGenerator.UpdateNormals(height, normals);
        this.UpdateMesh(mesh, meshDefinition, height, mesh.Bounds);
    }

    private Mesh GenerateMesh(IRWTexture height, float definition, BoundingBox bounds, string name)
    {
        var vertices = this.HeightMapGenerator.GenerateVertices(height, (int)(height.DimX * definition), (int)(height.DimY * definition));
        var indices = this.HeightMapGenerator.GenerateIndices((int)(height.DimX * definition), (int)(height.DimY * definition));
        return new Mesh(this.Device, bounds, vertices, indices, name, "mesh");
    }

    private void UpdateMesh(IMesh input, float definition, IRWTexture height, BoundingBox bounds)
    {
        this.HeightMapGenerator.UpdateVertices(height, (int)(height.DimX * definition), (int)(height.DimY * definition), input.Vertices);
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