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
public sealed class ErosionBrush : IDisposable
{
    private readonly Device Device;    
    private readonly Erosion Shader;
    private readonly Erosion.User User;

    public ErosionBrush(Device device, Erosion shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = shader.CreateUserFor<ErosionBrush>();
    }

    public void Apply(RWTexture2D height, RWTexture2D tint, int droplets)
    {
        var context = this.Device.ImmediateContext;

        const int brushWidth = 5;

        using var input = this.CreatePositionBuffer(height, droplets, context);
        using var brush = this.CreateBrushBuffer(brushWidth, context);

        context.CS.SetShaderResource(Erosion.Positions, input);
        context.CS.SetShaderResource(Erosion.Brush, brush);
        context.CS.SetConstantBuffer(Erosion.DropletConstantsSlot, this.User.DropletConstantsBuffer);
        context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(Erosion.MapTint, tint);
        context.CS.SetShader(this.Shader.Droplet);

        this.User.MapDropletConstants(context, (uint)height.Width, 3, (uint)droplets, (uint)brushWidth);

        var (x, y, z) = this.Shader.Droplet.GetDispatchSize(droplets, 1, 1);                
        context.CS.Dispatch(x, y, z);
    }

    private StructuredBuffer<Vector2> CreatePositionBuffer(RWTexture2D height, int droplets, DeviceContext context)
    {
        var input = new StructuredBuffer<Vector2>(this.Device, nameof(ErosionBrush));

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

    private StructuredBuffer<float> CreateBrushBuffer(int dimensions, DeviceContext context)
    {
        var buffer = new StructuredBuffer<float>(this.Device, nameof(ErosionBrush));

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