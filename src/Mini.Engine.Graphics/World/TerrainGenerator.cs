using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainGenerator
{
    private readonly Device Device;
    private readonly HeightMapGenerator HeightMapGenerator;
    private readonly HydraulicErosionBrush ErosionBrush;
    private readonly ContentManager Content;

    private static readonly Color4 Umber = new(140.0f / 255.0f, 105.0f / 255.0f, 75.0f / 255.0f);

    public TerrainGenerator(Device device, ContentManager content, HeightMapGenerator noiseGenerator, HydraulicErosionBrush erosionBrush)
    {
        this.Device = device;
        this.HeightMapGenerator = noiseGenerator;
        this.Content = content;
        this.ErosionBrush = erosionBrush;
    }
    // TODO: double check which resources should be tied to the content manager and/or should be disposed

    public TerrainMesh Generate(HeightMapGeneratorSettings settings, string name)
    {
        var height = this.HeightMapGenerator.GenerateHeights(settings);
        var normals = this.HeightMapGenerator.GenerateNormals(height);
        var tint = this.HeightMapGenerator.GenerateTint(settings.Dimensions, Umber);

        var bounds = ComputeBounds(settings.Amplitude, settings.Octaves, settings.Persistance);
        var mesh = this.GenerateMesh(height, bounds, name);        
        
        return new TerrainMesh(height, normals, tint, mesh);
    }

    public void Update(TerrainMesh input, HeightMapGeneratorSettings settings, string name)
{
        this.HeightMapGenerator.UpdateHeights(input.Height, settings);
        this.HeightMapGenerator.UpdateNormals(input.Height, input.Normals);
        this.HeightMapGenerator.UpdateTint(input.Tint, Umber);

        var bounds = ComputeBounds(settings.Amplitude, settings.Octaves, settings.Persistance);
        this.UpdateMesh(input.Mesh, input.Height, bounds);
    }

    public void Erode(TerrainMesh terrain, HydraulicErosionBrushSettings settings, string name)
    {
        this.ErosionBrush.Apply(terrain.Height, terrain.Tint, settings);
        this.HeightMapGenerator.UpdateNormals(terrain.Height, terrain.Normals);
        this.UpdateMesh(terrain.Mesh, terrain.Height, terrain.Mesh.Bounds);
    }

    private Mesh GenerateMesh(RWTexture2D height, BoundingBox bounds, string name)
    {
        var vertices = this.HeightMapGenerator.GenerateVertices(height);
        var indices = this.HeightMapGenerator.GenerateIndices(height.Width, height.Height);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, name, "mesh");
        this.Content.Link(mesh, $"{name}#terrain");

        return mesh;
    }

    private void UpdateMesh(Mesh input, RWTexture2D height, BoundingBox bounds)
    {
        var vertices = this.HeightMapGenerator.GenerateVertices(height);
        input.Vertices.MapData(this.Device.ImmediateContext, vertices);

        var indices = this.HeightMapGenerator.GenerateIndices(height.Width, height.Height);
        input.Indices.MapData(this.Device.ImmediateContext, indices);

        input.Bounds = bounds;
    }

    private static BoundingBox ComputeBounds(float amplitude, int octaves, float persistance)
    {
        var weight = 0.0f;
        for (var i = 0; i < octaves; i++)
        {
            weight += MathF.Pow(Math.Abs(persistance), i) * amplitude;
        }

        var min = new Vector3(-0.5f, -0.5f * weight, -0.5f);
        var max = new Vector3(0.5f, 0.5f * weight, 0.5f);
        return new BoundingBox(min, max);
    }
}