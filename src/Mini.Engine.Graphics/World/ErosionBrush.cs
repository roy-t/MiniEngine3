using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;
using Mini.Engine.DirectX.Contexts;

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

    public void Apply(RWTexture2D height, RWTexture2D normals)
    {
        var context = this.Device.ImmediateContext;        
        this.User.MapErosionConstants(context, (uint)height.Width);

        using var velocity = new RWTexture2D(this.Device, height.Width, height.Height, Format.R32G32B32A32_Float, false, nameof(ErosionBrush), "velocity");

        
        context.CS.SetConstantBuffer(Erosion.ErosionConstantsSlot, this.User.ErosionConstantsBuffer);
        context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(Erosion.MapNormal, normals);
        context.CS.SetUnorderedAccessView(Erosion.MapVelocity, velocity);

        this.Seed(context, height);

        context.CS.SetShader(this.Shader.Kernel);

        //var (x, y, z) = this.Shader.Kernel.GetDispatchSize(height.Width, height.Height, 1);

        //for (var i = 0; i < 100; i++)
        //{
        //    context.CS.Dispatch(x, y, z);
        //}
    }

    private void Seed(DeviceContext context, RWTexture2D height)
    {
        context.CS.SetShader(this.Shader.Seed);
        var (x, y, z) = this.Shader.Seed.GetDispatchSize(height.Width, height.Height, 1);
        context.CS.Dispatch(x, y, z);
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}