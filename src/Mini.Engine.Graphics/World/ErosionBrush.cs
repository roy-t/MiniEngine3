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

    public void Apply(RWTexture2D height, RWTexture2D tint, int iterations)
    {
        var context = this.Device.ImmediateContext;

        const int brushWidth = 5;

        using var input = this.CreatePositionBuffer(height, iterations, context);
        using var brush = this.CreateBrushBuffer(brushWidth, context);

        context.CS.SetShaderResource(Erosion.Positions, input);
        context.CS.SetShaderResource(Erosion.Brush, brush);
        context.CS.SetConstantBuffer(Erosion.DropletConstantsSlot, this.User.DropletConstantsBuffer);
        context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(Erosion.MapTint, tint);
        context.CS.SetShader(this.Shader.Droplet);

        this.User.MapDropletConstants(context, (uint)height.Width, 3, (uint)iterations, (uint)brushWidth);

        var (x, y, z) = this.Shader.Droplet.GetDispatchSize(iterations, 1, 1);                
        context.CS.Dispatch(x, y, z);
    }

    private StructuredBuffer<Vector2> CreatePositionBuffer(RWTexture2D height, int iterations, DeviceContext context)
    {
        var input = new StructuredBuffer<Vector2>(this.Device, nameof(ErosionBrush));

        var positions = new Vector2[iterations];
        for (var i = 0; i < iterations; i++)
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
        for(var x = 0; x < dimensions; x++)
        {
            for(var y = 0; y < dimensions; y++)
            {
                var index = Indexes.ToOneDimensional(x, y, dimensions);
                weights[index] = 0.5f;
            }
        }

        buffer.MapData(context, weights);
        return buffer;
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}