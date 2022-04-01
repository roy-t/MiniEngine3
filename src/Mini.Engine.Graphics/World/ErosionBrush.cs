using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Erosion;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class ErosionBrush
{
    private readonly Device Device;
    private readonly ConstantBuffer<ErosionConstants> Constants;
    private readonly ErosionKernel Kernel;

    public ErosionBrush(Device device, ErosionKernel kernel)
    {
        this.Device = device;
        this.Kernel = kernel;

        this.Constants = new ConstantBuffer<ErosionConstants>(device, $"{nameof(ErosionBrush)}_CB");
    }

    public void Apply(RWTexture2D height, RWTexture2D normals)
    {
        var context = this.Device.ImmediateContext;
        var cBuffer = new ErosionConstants()
        {
            Stride = (uint)height.Width
        };

        this.Constants.MapData(context, cBuffer);

        context.CS.SetShader(this.Kernel);
        context.CS.SetConstantBuffer(ErosionConstants.Slot, this.Constants);
        context.CS.SetUnorderedAccessView(Erosion.MapHeight, height);
        context.CS.SetUnorderedAccessView(Erosion.MapNormal, normals);

        var (x, y, z) = this.Kernel.GetDispatchSize(height.Width, height.Height, 1);
        context.CS.Dispatch(x, y, z);
    }
}