using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.HeightMapShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class HeightMapGenerator
{
    private readonly Device Device;
    private readonly ConstantBuffer<NoiseConstants> NoiseConstantBuffer;
    private readonly ConstantBuffer<IndicesConstants> IndicesConstantBuffer;
    private readonly HeightMapShaderNoiseKernel NoiseKernel;
    private readonly HeightMapShaderIndicesKernel IndicesKernel;

    public HeightMapGenerator(Device device, HeightMapShaderNoiseKernel noiseKernel, HeightMapShaderIndicesKernel indicesKernel)
    {
        this.Device = device;
        this.NoiseConstantBuffer = new ConstantBuffer<NoiseConstants>(device, $"{nameof(HeightMapGenerator)}_Noise_CB");
        this.IndicesConstantBuffer = new ConstantBuffer<IndicesConstants>(device, $"{nameof(HeightMapGenerator)}_Indices_CB");
        this.NoiseKernel = noiseKernel;
        this.IndicesKernel = indicesKernel;
    }

    /// <summary>
    /// Generates 2-dimensional simplex noise
    /// </summary>
    /// <param name="dimensions">Length of each dimension</param>
    /// <param name="amplitude">Amplitude of first octave</param>
    /// <param name="frequency">Frequency of first octave</param>
    /// <param name="octaves">Number of layers of noise</param>
    /// <param name="lacunarity">(1..) Increase in frequency for each consecutive octave, l * f ^ 0, l * f ^1, ...</param>
    /// <param name="persistance">[0..1), Decrease of amplitude for each consecutive octage, p * f ^ 0, p * f ^ 1, ...</param>
    /// <returns></returns>
    public (ModelVertex[], int[] indices) Generate(int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance)
    {
        var vertices = this.GenerateVertices(dimensions, offset, amplitude, frequency, octaves, lacunarity, persistance);
        var indices = this.GenerateIndices(dimensions);

        return (vertices, indices);
    }

    private ModelVertex[] GenerateVertices(int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance)
    {
        var context = this.Device.ImmediateContext;
        var cBuffer = new NoiseConstants()
        {
            Stride = (uint)dimensions,
            Offset = offset,
            Amplitude = amplitude,
            Frequency = frequency,
            Octaves = octaves,
            Lacunarity = lacunarity,
            Persistance = persistance
        };
        this.NoiseConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(NoiseConstants.Slot, this.NoiseConstantBuffer);

        var length = dimensions * dimensions;        
        using var output = new RWStructuredBuffer<ModelVertex>(this.Device, "output", length);

        context.CS.SetShader(this.NoiseKernel);
        context.CS.SetUnorderedAccessView(HeightMapShader.HeightMap, output);
        var (x, y, z) = this.NoiseKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        var data = new ModelVertex[length];
        output.ReadData(context, data);

        return data;
    }

    private int[] GenerateIndices(int dimensions)
    {
        var intervals = dimensions - 1;
        var quads = intervals * intervals;
        var triangles = quads * 2;
        var indices = triangles * 3;
        
        var context = this.Device.ImmediateContext;
        var cBuffer = new IndicesConstants()
        {
            Count = (uint)indices,
            Width = (uint)dimensions,
            Intervals = (uint)intervals
        };

        this.IndicesConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(IndicesConstants.Slot, this.IndicesConstantBuffer);

        using var output = new RWStructuredBuffer<int>(this.Device, "output", indices);

        context.CS.SetShader(this.IndicesKernel);
        context.CS.SetUnorderedAccessView(HeightMapShader.Indices, output);
        var (x, y, z) = this.NoiseKernel.GetDispatchSize(indices, 1, 1);
        context.CS.Dispatch(x, y, z);

        var data = new int[indices];
        output.ReadData(context, data);

        return data;
    }
}
