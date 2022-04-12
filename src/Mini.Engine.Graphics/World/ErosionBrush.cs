using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX.Resources;

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
        this.User = shader.CreateUser();
    }

    public void Apply(RWTexture2D height, RWTexture2D normals)
    {
        var context = this.Device.ImmediateContext;        
        this.User.MapErosionConstants(context, (uint)height.Width);
        

        context.CS.SetShader(this.Shader.Kernel);
        context.CS.SetConstantBuffer(Erosion.ErosionConstantsSlot, this.User.ErosionConstantsBuffer);
        context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(Erosion.MapNormal, normals);

        var (x, y, z) = this.Shader.Kernel.GetDispatchSize(height.Width, height.Height, 1);
        context.CS.Dispatch(x, y, z);
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}