using System;
using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Serilog;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainGenerator
{
    private readonly ILogger Logger;
    private readonly Device Device;
    private readonly HeightMapGenerator HeightMapGenerator;
    private readonly ErosionBrush ErosionBrush;
    private readonly ContentManager Content;

    public TerrainGenerator(ILogger logger, Device device, ContentManager content, HeightMapGenerator noiseGenerator, ErosionBrush erosionBrush)
    {
        this.Logger = logger.ForContext<TerrainGenerator>();
        this.Device = device;
        this.HeightMapGenerator = noiseGenerator;
        this.Content = content;
        this.ErosionBrush = erosionBrush;
    }

    public TerrainComponent Generate(Entity entity, int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance, string name)
    {
        var stopwatch = Stopwatch.StartNew();
        (var height, var normals) = this.HeightMapGenerator.GenerateMap(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance, "terrain");

        this.ErosionBrush.Apply(height, normals);

        var vertices = this.HeightMapGenerator.GenerateVertices(height, normals);
        var indices = this.HeightMapGenerator.GenerateIndices(height.Width, height.Height);

        this.Logger.Information("Terrain generator took {@miliseconds}", stopwatch.ElapsedMilliseconds);

        var weight = 0.0f;
        for(var i = 0; i < octaves; i++)
        {
            weight += MathF.Pow(Math.Abs(persistance), i) * amplitude;
        }

        var min = new Vector3(-0.5f, -0.5f * weight, -0.5f);
        var max = new Vector3(0.5f, 0.5f * weight, 0.5f);

        var bounds = new BoundingBox(min, max);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, $"{name}_mesh");
               
        this.Content.Link(mesh, $"{name}#terrain");

        return new TerrainComponent(entity, height, normals, mesh);
    }
}