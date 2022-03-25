using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Serilog;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainGenerator
{
    private readonly ILogger Logger;
    private readonly Device Device;
    private readonly SimplexNoiseGenerator NoiseGenerator;
    private readonly ContentManager Content;

    public TerrainGenerator(ILogger logger, Device device, SimplexNoiseGenerator noiseGenerator, ContentManager content)
    {
        this.Logger = logger.ForContext<TerrainGenerator>();
        this.Device = device;
        this.NoiseGenerator = noiseGenerator;
        this.Content = content;
    }

    public TerrainComponent Generate(Entity entity, int dimensions, string name)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();
        var heightMap = this.NoiseGenerator.Generate(dimensions);

        this.Logger.Information("Noise generator took {@miliseconds}", stopwatch.ElapsedMilliseconds);
                
        var texture = new Texture2D(this.Device, dimensions, dimensions, Format.R32_Float, false, $"{name}_heightmap");
        texture.SetPixels<float>(this.Device, heightMap);

        this.Logger.Information("Texture upload took {@miliseconds}", stopwatch.ElapsedMilliseconds);
        stopwatch.Restart();
        
        (var indices, var vertices, var bounds) = HeightMapTriangulator.Triangulate(heightMap, dimensions);
        var mesh = new Mesh(this.Device, bounds, vertices, indices, $"{name}_mesh");

        this.Logger.Information("Triangulation took {@miliseconds}", stopwatch.ElapsedMilliseconds);

        this.Content.Link(texture, $"{name}#texture");
        this.Content.Link(mesh, $"{name}#terrain");

        return new TerrainComponent(entity, texture, mesh);
    }
}