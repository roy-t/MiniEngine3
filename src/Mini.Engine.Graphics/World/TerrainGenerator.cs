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
    private readonly HeightMapGenerator NoiseGenerator;
    private readonly ContentManager Content;

    public TerrainGenerator(ILogger logger, Device device, HeightMapGenerator noiseGenerator, ContentManager content)
    {
        this.Logger = logger.ForContext<TerrainGenerator>();
        this.Device = device;
        this.NoiseGenerator = noiseGenerator;
        this.Content = content;
    }

    public TerrainComponent Generate(Entity entity, int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance, string name)
    {
        var stopwatch = Stopwatch.StartNew();
        (var vertices, var indices) = this.NoiseGenerator.Generate(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance);

        this.Logger.Information("Noise generator took {@miliseconds}", stopwatch.ElapsedMilliseconds);

        // TODO: looks like noise is larger than 1*amplitude?
        var min = new Vector3(-0.5f, -amplitude, -0.5f);
        var max = new Vector3(0.5f, amplitude, 0.5f);

        var bounds = new BoundingBox(min, max);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, $"{name}_mesh");
               
        this.Content.Link(mesh, $"{name}#terrain");

        return new TerrainComponent(entity, mesh);
    }
}