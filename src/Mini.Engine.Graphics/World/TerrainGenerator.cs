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
    private readonly ContentManager Content;

    public TerrainGenerator(ILogger logger, Device device, HeightMapGenerator noiseGenerator, ContentManager content)
    {
        this.Logger = logger.ForContext<TerrainGenerator>();
        this.Device = device;
        this.HeightMapGenerator = noiseGenerator;
        this.Content = content;
    }

    public TerrainComponent Generate(Entity entity, int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance, string name)
    {
        // Plan with terrain generator
        // 1. NoiseGenerator should generate two texture, one with height and one with normals
        // 2. A separate shader should take the two textures and turn them into vertices/indices
        // 3. In between other shaders can manipulate it, each should generate a new texture
        // so you can easily see the inbetween steps (for now at least)

        var stopwatch = Stopwatch.StartNew();
        //(var vertices, var indices) = this.HeightMapGenerator.Generate(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance);
        (var height, var normals) = this.HeightMapGenerator.GenerateMap(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance, "terrain");
        var vertices = this.HeightMapGenerator.GenerateVertices(height, normals);
        var indices = this.HeightMapGenerator.GenerateIndices(height.Width, height.Height);

        this.Logger.Information("Noise generator took {@miliseconds}", stopwatch.ElapsedMilliseconds);

        // TODO: looks like noise is larger than 1*amplitude?
        var min = new Vector3(-0.5f, -amplitude, -0.5f);
        var max = new Vector3(0.5f, amplitude, 0.5f);

        var bounds = new BoundingBox(min, max);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, $"{name}_mesh");
               
        this.Content.Link(mesh, $"{name}#terrain");

        return new TerrainComponent(entity, height, normals, mesh);
    }
}