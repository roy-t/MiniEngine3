using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS;
using Vortice.DXGI;

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
    public RWTexture2D GenerateHeights(int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance, Entity user)
    {
        var context = this.Device.ImmediateContext;

        this.User.MapNoiseConstants(context, (uint)dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        var height = new RWTexture2D(this.Device, dimensions, dimensions, Format.R32_Float, false, user.ToString(), "HeightMap");

        context.CS.SetShader(this.Shader.NoiseMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, height);

        var (x, y, z) = this.Shader.NoiseMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        return height;
    }

    public RWTexture2D GenerateNormals(RWTexture2D heightMap, Entity user)
    {
        var dimensions = heightMap.Width;

        var context = this.Device.ImmediateContext;

        this.User.MapNoiseConstants(context, (uint)dimensions, Vector2.Zero, 0, 0, 0, 0, 0);
        context.CS.SetConstantBuffer(HeightMap.NoiseConstantsSlot, this.User.NoiseConstantsBuffer);

        var normals = new RWTexture2D(this.Device, dimensions, dimensions, Format.R32G32B32A32_Float, false, user.ToString(), "NormalMap");

        context.CS.SetShader(this.Shader.NormalMapKernel);
        context.CS.SetUnorderedAccessView(HeightMap.MapHeight, heightMap);
        context.CS.SetUnorderedAccessView(HeightMap.MapNormal, normals);

        var (x, y, z) = this.Shader.NormalMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        return normals;
    }

    public ModelVertex[] GenerateVertices(RWTexture2D heightMap, RWTexture2D normalMap)
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
