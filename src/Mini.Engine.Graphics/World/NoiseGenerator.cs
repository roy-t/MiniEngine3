using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.NoiseShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class NoiseGenerator
{
    private readonly Device Device;
    private readonly ConstantBuffer<Constants> ConstantBuffer;
    private readonly NoiseShaderKernel Kernel;

    public NoiseGenerator(Device device, NoiseShaderKernel noiseShader)
    {
        this.Device = device;
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(NoiseGenerator)}_CB");
        this.Kernel = noiseShader;
    }

    public float[] Generate(int dimensions)
    {
        var context = this.Device.ImmediateContext;

        var vertices = new float[dimensions * dimensions];
        var cBuffer = new Constants()
        {
            Stride = (uint)dimensions
        };
        this.ConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

        using var input = new StructuredBuffer<float>(this.Device, "input");
        input.MapData(context, vertices);

        using var output = new RWStructuredBuffer<float>(this.Device, "output", vertices.Length);

        context.CS.SetShader(this.Kernel);
        context.CS.SetShaderResource(NoiseShader.Tile, input);
        context.CS.SetUnorderedAccessView(NoiseShader.World, output);

        var (x, y, z) = this.Kernel.GetDispatchSize(dimensions, dimensions, 1);
        context.CS.Dispatch(x, y, z);

        var data = new float[vertices.Length];
        output.ReadData(context, data);

        return data;
    }
}
