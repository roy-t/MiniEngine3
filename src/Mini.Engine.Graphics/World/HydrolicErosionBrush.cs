using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Buffers;
using System.Numerics;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Core;

namespace Mini.Engine.Graphics.World;

public sealed class HydrolicErosionBrushSettings
{
    /// <summary>
    /// Number of simulated droplets
    /// </summary>
    public int Droplets;

    /// <summary>
    /// Size of an individual droplet
    /// </summary>
    public int DropletStride;

    /// <summary>
    /// Multiplier for the amount of sediment one droplet of water can carry. Lower numbers produce a softer effect. Higher
    // numbers produce a stronger effect.
    // Range: [0.01f..5.0f]
    /// </summary>
    public float SedimentFactor;

    /// <summary>
    /// Sediment capacity of slow moving or standing still water. Lower numbers prevent cratering but might stop a droplet
    /// from affecting the terrain before the end of its lifetime. Higher numbers sometimes lead to craters and hills forming
    /// on flat surfaces.
    /// Range: [0..0.01]
    /// </summary>
    public float MinSedimentCapacity;

    /// <summary>
    /// Minimum speed, in meters per second, that water flows at. The speed of the water affects its sediment capacity.
    /// Lower numbers create more deposits and thus a rougher terrain. Higher numbers create a smoother terrain with more erosion.
    /// Range: [0.0025f..1.0f]
    /// </summary>
    public float MinSpeed;

    /// <summary>
    ///Maximum speed, in meters per second, that water flows at. The speed of the water affects its sediment capacity.
    // Lower numbers create more deposits and thus a rougher terrain. Higher numbers create a smoother terrain with more erosion.
    // Range: [1.0f..10.0f]
    /// </summary>
    public float MaxSpeed;

    /// <summary>
    /// Inertia. 
    /// Controls how much water keeps going the same direction. Lower numbers make the water follow the contours of the 
    /// terrain better. Higher numbers allow the water to maintain its momentum and even allow it to flow slightly up
    /// Range: [0..1]
    /// </summary>
    public float Inertia;

    /// <summary>
    ///Affects the acceleration over time of water that is going up or down hill. Lower numbers reduce the effect on steep terrain.
    // Higher numbers increase the effect on steep terrain.
    // Range [1.0f, 20.0f]
    /// </summary>
    public float Gravity;

    public HydrolicErosionBrushSettings(int droplets = 1_000_000, int dropletStride = 5, float sedimentFactor = 1.0f, float minSedimentCapacity = 0.001f, float minSpeed = 0.01f, float maxSpeed = 7.0f, float inertia = 0.55f, float gravity = 4.0f)
    {
        this.Droplets = droplets;
        this.DropletStride = dropletStride;
        this.SedimentFactor = sedimentFactor;
        this.MinSedimentCapacity= minSedimentCapacity;
        this.MinSpeed = minSpeed;
        this.MaxSpeed = maxSpeed;
        this.Inertia = inertia;
        this.Gravity = gravity;
    }
}


[Service]
public sealed class HydrolicErosionBrush : IDisposable
{
    private readonly Device Device;    
    private readonly HydrolicErosion Shader;
    private readonly HydrolicErosion.User User;

    public HydrolicErosionBrush(Device device, HydrolicErosion shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = shader.CreateUserFor<HydrolicErosionBrush>();
    }

    public void Apply(RWTexture2D height, RWTexture2D tint, HydrolicErosionBrushSettings settings)
    {
        var context = this.Device.ImmediateContext;        

        using var input = this.CreatePositionBuffer(height, settings.Droplets, context);
        using var dropletMask = this.CreateDropletMaskBuffer(settings.DropletStride, context);

        context.CS.SetShaderResource(HydrolicErosion.Positions, input);
        context.CS.SetShaderResource(HydrolicErosion.DropletMask, dropletMask);
        context.CS.SetConstantBuffer(HydrolicErosion.ConstantsSlot, this.User.ConstantsBuffer);
        context.CS.SetUnorderedAccessView(HydrolicErosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(HydrolicErosion.MapTint, tint);
        context.CS.SetShader(this.Shader.Kernel);

        //this.User.MapConstants(context, (uint)height.Width, 3, (uint)droplets, (uint)brushWidth);

        this.User.MapConstants(context, (uint)height.Width, (uint)settings.DropletStride / 2u, (uint)settings.Droplets, (uint)settings.DropletStride,
            settings.Inertia, settings.MinSedimentCapacity, settings.MinSpeed, settings.MaxSpeed, settings.Gravity, settings.SedimentFactor);

        var (x, y, z) = this.Shader.Kernel.GetDispatchSize((int)settings.Droplets, 1, 1);                
        context.CS.Dispatch(x, y, z);
    }

    private StructuredBuffer<Vector2> CreatePositionBuffer(RWTexture2D height, int droplets, DeviceContext context)
    {
        var input = new StructuredBuffer<Vector2>(this.Device, nameof(HydrolicErosionBrush));

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
        var buffer = new StructuredBuffer<float>(this.Device, nameof(HydrolicErosionBrush));

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