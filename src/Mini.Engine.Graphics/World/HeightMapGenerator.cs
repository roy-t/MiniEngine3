using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class HeightMapGenerator : IDisposable
{
    private readonly Device Device;
    private readonly HeightMap Shader;
    private readonly HeightMap.User User;

    public HeightMapGenerator(Device device, HeightMap shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = this.Shader.CreateUserFor<HeightMapGenerator>();
    }

    /// <summary>
    /// Generates 2-dimensional height map and its corresponding normal map using simplex noise
    /// </summary>
    /// <param name="dimensions">Length of each dimension</param>
    /// <param name="amplitude">Amplitude of first octave</param>
    /// <param name="frequency">Frequency of first octave</param>
    /// <param name="octaves">Number of layers of noise</param>
    /// <param name="lacunarity">(1..) Increase in frequency for each consecutive octave, l * f ^ 0, l * f ^1, ...</param>
    /// <param name="persistance">[0..1), Decrease of amplitude for each consecutive octage, p * f ^ 0, p * f ^ 1, ...</param>
    /// <returns></returns>
    public RWTexture2D GenerateHeights(int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance)
    {        
        var height = new RWTexture2D(this.Device, dimensions, dimensions, Format.R32_Float, false, nameof(HeightMapGenerator), "HeightMap");
        this.UpdateHeights(height, offset, amplitude, frequency, octaves, lacunarity, persistance);

        return height;
    }   

    public void UpdateHeights(RWTexture2D height, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance)
    {
        var context = this.Device.ImmediateContext;
        var dimensions = height.Width;

        this.User.MapNoiseConstants(context, (uint)dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        context.CS.SetShader(this.Shader.NoiseMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, height);

        var (x, y, z) = this.Shader.NoiseMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);        
    }

    public RWTexture2D GenerateNormals(RWTexture2D heightMap)
    {
        var dimensions = heightMap.Width;
        var normals = new RWTexture2D(this.Device, dimensions, dimensions, Format.R32G32B32A32_Float, false, nameof(HeightMapGenerator), "NormalMap");

        this.UpdateNormals(heightMap, normals);

        return normals;
    }

    public void UpdateNormals(RWTexture2D heightMap, RWTexture2D normals)
    {
        var context = this.Device.ImmediateContext;
        var dimensions = normals.Width;

        this.User.MapNoiseConstants(context, (uint)dimensions, Vector2.Zero, 0, 0, 0, 0, 0);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        context.CS.SetShader(this.Shader.NormalMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, heightMap);
        context.CS.SetUnorderedAccessView(HeightMap.MapNormal, normals);

        var (x, y, z) = this.Shader.NormalMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);
    }

    public RWTexture2D GenerateTint(int dimensions, Color4 tint)
    {
        var texture = new RWTexture2D(this.Device, dimensions, dimensions, Format.R8G8B8A8_UNorm, false, nameof(HeightMapGenerator), "Tint");
        this.UpdateTint(texture, tint);

        return texture;
    }

    public void UpdateTint(RWTexture2D texture, Color4 tint)
{
        this.Device.ImmediateContext.Clear(texture, tint);        
    }

    public ModelVertex[] GenerateVertices(RWTexture2D heightMap)
    {
        var context = this.Device.ImmediateContext;
        
        this.User.MapTriangulateConstants(context, (uint)heightMap.Width, (uint)heightMap.Height, 0, 0);
        context.CS.SetConstantBuffer(HeightMap.TriangulateConstantsSlot, this.User.TriangulateConstantsBuffer);
        var length = heightMap.Width * heightMap.Height;        
        using var output = new RWStructuredBuffer<ModelVertex>(this.Device, nameof(HeightMapGenerator), length);

        context.CS.SetShader(this.Shader.TriangulateKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, heightMap);
        context.CS.SetUnorderedAccessView(HeightMap.Vertices, output);
        var (x, y, z) = this.Shader.TriangulateKernel.GetDispatchSize(heightMap.Width, heightMap.Height, 1);
        context.CS.Dispatch(x, y, z);

        var data = new ModelVertex[length];
        output.ReadData(context, data);

        return data;
    }

    public int[] GenerateIndices(int width, int height)
    {
        var context = this.Device.ImmediateContext;

        var intervals = width - 1;
        var quads = intervals * intervals;
        var triangles = quads * 2;
        var indices = triangles * 3;

        this.User.MapTriangulateConstants(context, (uint)width, (uint)height, (uint)indices, (uint)intervals);
        context.CS.SetConstantBuffer(HeightMap.TriangulateConstantsSlot, this.User.TriangulateConstantsBuffer);

        using var output = new RWStructuredBuffer<int>(this.Device, nameof(HeightMapGenerator), indices);

        context.CS.SetShader(this.Shader.IndicesKernel);
        context.CS.SetUnorderedAccessView(HeightMap.Indices, output);
        var (x, y, z) = this.Shader.IndicesKernel.GetDispatchSize(indices, 1, 1);
        context.CS.Dispatch(x, y, z);

        var data = new int[indices];
        output.ReadData(context, data);

        return data;
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}
