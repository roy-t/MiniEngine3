using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.HeightMapShader;
using Mini.Engine.Core;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class HeightMapGenerator
{
    private readonly Device Device;
    private readonly ConstantBuffer<NoiseConstants> NoiseConstantBuffer;
    private readonly ConstantBuffer<TriangulateConstants> TriangulateConstantBuffer;
    private readonly ConstantBuffer<IndicesConstants> IndicesConstantBuffer;

    private readonly HeightMapShaderNoiseMapKernel NoiseMapKernel;
    private readonly HeightMapShaderTriangulateKernel TriangulateKernel;
    private readonly HeightMapShaderIndicesKernel IndicesKernel;

    public HeightMapGenerator(Device device, HeightMapShaderNoiseMapKernel noiseMapKernel, HeightMapShaderTriangulateKernel triangulateKernel, HeightMapShaderIndicesKernel indicesKernel)
    {
        this.Device = device;
        this.NoiseConstantBuffer = new ConstantBuffer<NoiseConstants>(device, $"{nameof(HeightMapGenerator)}_Noise_CB");
        this.TriangulateConstantBuffer = new ConstantBuffer<TriangulateConstants>(device, $"{nameof(HeightMapGenerator)}_Triangulate_CB");
        this.IndicesConstantBuffer = new ConstantBuffer<IndicesConstants>(device, $"{nameof(HeightMapGenerator)}_Indices_CB");
        this.NoiseMapKernel = noiseMapKernel;
        this.TriangulateKernel = triangulateKernel;
        this.IndicesKernel = indicesKernel;
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
    public (RWTexture2D height, RWTexture2D normals) GenerateMap(int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance, string name)
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

        var height = new RWTexture2D(this.Device, dimensions, dimensions, Format.R32_Float, false, $"{name}_heightmap");
        var normals = new RWTexture2D(this.Device, dimensions, dimensions, Format.R32G32B32A32_Float, false, $"{name}_normalmap");

        context.CS.SetShader(this.NoiseMapKernel);
        context.CS.SetUnorderedAccessView(HeightMapShader.NoiseMapHeight, height);
        context.CS.SetUnorderedAccessView(HeightMapShader.NoiseMapNormal, normals);

        var (x, y, z) = this.NoiseMapKernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);


        return (height, normals);
    }   

    public ModelVertex[] GenerateVertices(RWTexture2D heightMap, RWTexture2D normalMap)
    {
        var context = this.Device.ImmediateContext;
        var cBuffer = new TriangulateConstants()
{
            Width = (uint)heightMap.Width,
            Height = (uint)heightMap.Height
        };
        this.TriangulateConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(TriangulateConstants.Slot, this.TriangulateConstantBuffer);
        var length = heightMap.Width * heightMap.Height;        
        using var output = new RWStructuredBuffer<ModelVertex>(this.Device, "output", length);

        context.CS.SetShader(this.TriangulateKernel);
        context.CS.SetUnorderedAccessView(HeightMapShader.NoiseMapHeight, heightMap);
        context.CS.SetUnorderedAccessView(HeightMapShader.NoiseMapNormal, normalMap);
        context.CS.SetUnorderedAccessView(HeightMapShader.HeightMap, output);
        var (x, y, z) = this.TriangulateKernel.GetDispatchSize(heightMap.Width, heightMap.Height, 1);
        context.CS.Dispatch(x, y, z);

        var data = new ModelVertex[length];
        output.ReadData(context, data);

        return data;
    }

    public int[] GenerateIndices(int width, int height)
    {
        var intervals = width - 1;
        var quads = intervals * intervals;
        var triangles = quads * 2;
        var indices = triangles * 3;

        var context = this.Device.ImmediateContext;
        var cBuffer = new IndicesConstants()
        {
            Count = (uint)indices,
            Intervals = (uint)intervals,
            Nwidth = (uint)width,
            Nheight = (uint)height,            
        };

        this.IndicesConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(IndicesConstants.Slot, this.IndicesConstantBuffer);

        using var output = new RWStructuredBuffer<int>(this.Device, "output", indices);

        context.CS.SetShader(this.IndicesKernel);
        context.CS.SetUnorderedAccessView(HeightMapShader.Indices, output);
        var (x, y, z) = this.IndicesKernel.GetDispatchSize(indices, 1, 1);
        context.CS.Dispatch(x, y, z);

        var data = new int[indices];
        output.ReadData(context, data);

        return data;
    }
}
