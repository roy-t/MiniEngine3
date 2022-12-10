using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Core;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class HydraulicErosionBrush : IDisposable
{
    private readonly Device Device;
    private readonly HydraulicErosion Shader;
    private readonly HydraulicErosion.User User;

    public HydraulicErosionBrush(Device device, HydraulicErosion shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = shader.CreateUserFor<HydraulicErosionBrush>();
    }

    public void Apply(IRWTexture height, IRWTexture tint, HydraulicErosionBrushSettings settings)
    {
        var context = this.Device.ImmediateContext;

        using var input = this.CreatePositionBuffer(height, settings.Seed, settings.Droplets, context);
        using var dropletMask = this.CreateDropletMaskBuffer(settings.DropletStride, context);

        context.CS.SetShaderResource(HydraulicErosion.Positions, input);
        context.CS.SetShaderResource(HydraulicErosion.DropletMask, dropletMask);
        context.CS.SetConstantBuffer(HydraulicErosion.ConstantsSlot, this.User.ConstantsBuffer);
        context.CS.SetUnorderedAccessView(HydraulicErosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(HydraulicErosion.MapTint, tint);
        context.CS.SetShader(this.Shader.Kernel);

        this.User.MapConstants(context, (uint)height.DimX, (uint)Math.Ceiling(settings.DropletStride / 2.0f), (uint)settings.Droplets, (uint)settings.DropletStride,
            settings.Inertia, settings.MinSedimentCapacity, settings.Gravity, settings.SedimentFactor, settings.DepositSpeed);

        // TODO: dispatch dimension must be below D3D11_CS_DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION for higher than 1M dorplets
        // we need to tweak numthreads or dispatch multiple times
        var (x, y, z) = this.Shader.GetDispatchSizeForKernel(settings.Droplets, 1, 1);
        context.CS.Dispatch(x, y, z);

        context.CS.ClearUnorderedAccessView(HydraulicErosion.MapHeight);
        context.CS.ClearUnorderedAccessView(HydraulicErosion.MapTint);
    }

    private StructuredBuffer<Vector2> CreatePositionBuffer(IRWTexture height, int seed, int droplets, DeviceContext context)
    {
        var input = new StructuredBuffer<Vector2>(this.Device, nameof(HydraulicErosionBrush));

        var positions = GenerateRandomPositions(height.DimX, height.DimY, seed, droplets);
        //var positions = GenerateQuasiRandomPositions(height.DimX, height.DimY, seed, droplets);

        input.MapData(context, positions);
        return input;
    }

    private static Vector2[] GenerateQuasiRandomPositions(int dimX, int dimY, int seed, int length)
    {
        var sequence = new QuasiRandomSequence();
        var positions = new Vector2[length];
        for (var i = 0; i < length; i++)
        {
            positions[i] = sequence.Next2D(0, dimX, 0, dimY) + new Vector2(0.5f, 0.5f);
        }

        return positions;
    }

    private static Vector2[] GenerateRandomPositions(int dimX, int dimY, int seed, int length)
    {
        var random = new Random(seed);
        var positions = new Vector2[length];
        for (var i = 0; i < length; i++)
        {
            var startX = random.Next(0, dimX) + 0.5f;
            var startY = random.Next(0, dimY) + 0.5f;

            positions[i] = new Vector2(startX, startY);
        }

        return positions;
    }

    private StructuredBuffer<float> CreateDropletMaskBuffer(int dimensions, DeviceContext context)
    {
        var buffer = new StructuredBuffer<float>(this.Device, nameof(HydraulicErosionBrush));

        var weights = new float[dimensions * dimensions];

        var center = MathF.Floor(dimensions / 2.0f);
        var maxDistance = MathF.Sqrt(center * center + center * center);

        var sum = 0.0f;
        for (var y = 0; y < dimensions; y++)
        {
            for (var x = 0; x < dimensions; x++)
            {
                var index = Indexes.ToOneDimensional(x, y, dimensions);

                var distance = MathF.Sqrt(((center - x) * (center - x)) + ((center - y) * (center - y)));
                var weight = maxDistance - distance;
                sum += weight;
                weights[index] = weight;
            }
        }

        for (var i = 0; i < weights.Length; i++)
        {
            weights[i] /= sum;
        }

        buffer.MapData(context, weights);
        return buffer;
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}