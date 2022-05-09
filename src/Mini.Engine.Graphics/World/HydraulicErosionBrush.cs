using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Buffers;
using System.Numerics;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Core;

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

    public void Apply(RWTexture2D height, RWTexture2D tint, HydraulicErosionBrushSettings settings)
    {
        var context = this.Device.ImmediateContext;        

        using var input = this.CreatePositionBuffer(height, settings.Droplets, context);
        using var dropletMask = this.CreateDropletMaskBuffer(settings.DropletStride, context);

        context.CS.SetShaderResource(HydraulicErosion.Positions, input);
        context.CS.SetShaderResource(HydraulicErosion.DropletMask, dropletMask);
        context.CS.SetConstantBuffer(HydraulicErosion.ConstantsSlot, this.User.ConstantsBuffer);
        context.CS.SetUnorderedAccessView(HydraulicErosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(HydraulicErosion.MapTint, tint);
        context.CS.SetShader(this.Shader.Kernel);

        this.User.MapConstants(context, (uint)height.Width, (uint)Math.Ceiling(settings.DropletStride / 2.0f), (uint)settings.Droplets, (uint)settings.DropletStride,
            settings.Inertia, settings.MinSedimentCapacity, settings.Gravity, settings.SedimentFactor, settings.DepositSpeed);

        var (x, y, z) = this.Shader.Kernel.GetDispatchSize((int)settings.Droplets, 1, 1);                
        context.CS.Dispatch(x, y, z);
    }

    private StructuredBuffer<Vector2> CreatePositionBuffer(RWTexture2D height, int droplets, DeviceContext context)
    {
        var input = new StructuredBuffer<Vector2>(this.Device, nameof(HydraulicErosionBrush));

        var positions = new Vector2[droplets];
        for (var i = 0; i < droplets; i++)
        {
            var startX = Random.Shared.Next(0, height.Width) + 0.5f;
            var startY = Random.Shared.Next(0, height.Height) + 0.5f;

            positions[i] = new Vector2(startX, startY);
        }

        input.MapData(context, positions);
        return input;
    }

    private StructuredBuffer<float> CreateDropletMaskBuffer(int dimensions, DeviceContext context)
    {
        var buffer = new StructuredBuffer<float>(this.Device, nameof(HydraulicErosionBrush));

        var weights = new float[dimensions * dimensions];

        var center = MathF.Floor(dimensions / 2.0f);
        var maxDistance = MathF.Sqrt(center * center + center * center);

        var sum = 0.0f;
        for(var y = 0; y < dimensions; y++)
        {
            for(var x = 0; x < dimensions; x++)
            {
                var index = Indexes.ToOneDimensional(x, y, dimensions);

                var distance = MathF.Sqrt(((center - x) * (center - x)) + ((center - y) * (center - y)));
                var weight = maxDistance - distance;
                sum += weight;                
                weights[index] = weight;
            }
        }

        for(var i = 0; i < weights.Length; i++)
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