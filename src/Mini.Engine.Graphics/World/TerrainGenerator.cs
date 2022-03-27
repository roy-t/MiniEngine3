using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Serilog;
using Vortice.DXGI;
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
        var stopwatch = new Stopwatch();

        stopwatch.Restart();
        (var vertices, var indices) = this.NoiseGenerator.Generate(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance);

        this.Logger.Information("Noise generator took {@miliseconds}", stopwatch.ElapsedMilliseconds);
                
        var texture = new Texture2D(this.Device, dimensions, dimensions, Format.R32_Float, false, $"{name}_heightmap");
        //texture.SetPixels<float>(this.Device, heightMap);

        this.Logger.Information("Texture upload took {@miliseconds}", stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();

        //(var indices, var vertices, var bounds) = HeightMapTriangulator.Triangulate(heightMap, dimensions);

        var bounds = new BoundingBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
        //var indices = HeightMapTriangulator.CalculateIndicesPlain(dimensions);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, $"{name}_mesh");

        this.Logger.Information("Triangulation took {@miliseconds}", stopwatch.ElapsedMilliseconds);

        this.Content.Link(texture, $"{name}#texture");
        this.Content.Link(mesh, $"{name}#terrain");

        return new TerrainComponent(entity, texture, mesh);
    }
}