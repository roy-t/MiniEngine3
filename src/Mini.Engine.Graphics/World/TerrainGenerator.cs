using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics.World;


public record Terrain(ITexture2D HeightMap, IModel Mesh);

[Service]
public sealed class TerrainGenerator
{
    private readonly Device Device;
    private readonly SimplexNoiseGenerator NoiseGenerator;
    private readonly ContentManager Content;

    public TerrainGenerator(Device device, SimplexNoiseGenerator noiseGenerator, ContentManager content)
    {
        this.Device = device;
        this.NoiseGenerator = noiseGenerator;
        this.Content = content;
    }

    public Terrain Generate(int dimensions, string name)
    {
        var material = this.Content.LoadDefaultMaterial();

        var heightMap = this.NoiseGenerator.Generate(dimensions);

        var texture = new Texture2D(this.Device, dimensions, dimensions, Vortice.DXGI.Format.R32_Float, false, $"{name}_heightmap");
        texture.SetPixels<float>(this.Device, heightMap);
        var mesh = HeightMapTriangulator.Triangulate(this.Device, heightMap, dimensions, material, $"{name}_mesh");

        this.Content.Link(texture, $"{name}#texture");
        this.Content.Link(mesh, $"{name}#terrain");

        return new Terrain(texture, mesh);
    }
}