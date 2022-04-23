using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;
using Mini.Engine.DirectX.Contexts;
using System;

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

    public void Apply(RWTexture2D height, int iterations)
    {      
        var context = this.Device.ImmediateContext;        

        context.CS.SetConstantBuffer(Erosion.DropletConstantsSlot, this.User.DropletConstantsBuffer);
        context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        context.CS.SetShader(this.Shader.Droplet);

        var (x, y, z) = this.Shader.Droplet.GetDispatchSize(1, 1, 1);

        for (var i = 0; i < iterations; i++)
        {
            var startX = Random.Shared.NextSingle() * height.Width;
            var startY = Random.Shared.NextSingle() *height.Height;

            this.User.MapDropletConstants(context, startX, startY, (uint)height.Width);
            context.CS.Dispatch(x, y, z);
        }


        //using var velocityA = new RWTexture2D(this.Device, height.Width, height.Height, Format.R32G32B32A32_Float, false, nameof(ErosionBrush), "velocityA");
        //using var velocityB = new RWTexture2D(this.Device, height.Width, height.Height, Format.R32G32B32A32_Float, false, nameof(ErosionBrush), "velocityB");

        //using var massA = new RWTexture2D(this.Device, height.Width, height.Height, Format.R32G32B32A32_Float, false, nameof(ErosionBrush), "velocityA");
        //using var massB = new RWTexture2D(this.Device, height.Width, height.Height, Format.R32G32B32A32_Float, false, nameof(ErosionBrush), "velocityB");

        //var velocityIn = velocityA;
        //var velocityOut = velocityB;

        //var massIn = massA;
        //var massOut = massB;

        //context.CS.SetConstantBuffer(Erosion.ErosionConstantsSlot, this.User.ErosionConstantsBuffer);
        //context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        //context.CS.SetUnorderedAccessView(Erosion.MapVelocityIn, velocityIn);
        //context.CS.SetUnorderedAccessView(Erosion.MapVelocityOut, velocityOut);
        //context.CS.SetUnorderedAccessView(Erosion.MapMassIn, massIn);
        //context.CS.SetUnorderedAccessView(Erosion.MapMassOut, massOut);

        //this.Seed(context, height);

        //context.CS.SetShader(this.Shader.Erode);

        //var (x, y, z) = this.Shader.Erode.GetDispatchSize(height.Width, height.Height, 1);

        //for (var i = 0; i < iterations; i++)
        //{
        //    (velocityOut, velocityIn) = (velocityIn, velocityOut);
        //    (massOut, massIn) = (massIn, massOut);
        //    context.CS.SetUnorderedAccessView(Erosion.MapVelocityIn, velocityIn);
        //    context.CS.SetUnorderedAccessView(Erosion.MapVelocityOut, velocityOut);
        //    context.CS.SetUnorderedAccessView(Erosion.MapMassIn, massIn);
        //    context.CS.SetUnorderedAccessView(Erosion.MapMassOut, massOut);
        //    context.CS.Dispatch(x, y, z);
        //}
    }

    //private void Seed(DeviceContext context, RWTexture2D height)
    //{
    //    context.CS.SetShader(this.Shader.Seed);
    //    var (x, y, z) = this.Shader.Seed.GetDispatchSize(height.Width, height.Height, 1);
    //    context.CS.Dispatch(x, y, z);
    //}

    public void Dispose()
    {
        this.User.Dispose();
    }
}