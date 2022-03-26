using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.NoiseShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class SimplexNoiseGenerator
{
    private readonly Device Device;
    private readonly ConstantBuffer<Constants> ConstantBuffer;
    private readonly NoiseShaderKernel Kernel;

    public SimplexNoiseGenerator(Device device, NoiseShaderKernel noiseShader)
    {
        this.Device = device;
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(SimplexNoiseGenerator)}_CB");
        this.Kernel = noiseShader;
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
    public float[] Generate(int dimensions, Vector2 offset, float amplitude, float frequency, int octaves, float lacunarity, float persistance)
    {
        var context = this.Device.ImmediateContext;

        var length = dimensions * dimensions;
        var cBuffer = new Constants()
        {            
            Stride = (uint)dimensions,
            Offset = offset,
            Amplitude = amplitude,
            Frequency = frequency,
            Octaves = octaves,
            Lacunarity = lacunarity,
            Persistance = persistance
        };
        this.ConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

        using var output = new RWStructuredBuffer<float>(this.Device, "output", length);

        context.CS.SetShader(this.Kernel);
        context.CS.SetUnorderedAccessView(NoiseShader.World, output);

        var (x, y, z) = this.Kernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        var data = new float[length];
        output.ReadData(context, data);

        return data;
    }
}
