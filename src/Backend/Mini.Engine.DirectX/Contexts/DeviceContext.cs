using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public abstract class DeviceContext : IDisposable
{
    internal DeviceContext(Device device, ID3D11DeviceContext context, string name)
    {
        this.Device = device;
        this.ID3D11DeviceContext = context;
        this.ID3D11DeviceContext.DebugName = name;

        this.IA = new InputAssemblerContext(this);
        this.VS = new VertexShaderContext(this);
        this.PS = new PixelShaderContext(this);
        this.OM = new OutputMergerContext(this);
        this.RS = new RasterizerContext(this);
    }

    public InputAssemblerContext IA { get; }
    public VertexShaderContext VS { get; }
    public PixelShaderContext PS { get; }
    public OutputMergerContext OM { get; }
    public RasterizerContext RS { get; }

    public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
    {
        this.ID3D11DeviceContext.DrawIndexed(indexCount, indexOffset, vertexOffset);
    }

    public Device Device { get; }
    internal ID3D11DeviceContext ID3D11DeviceContext { get; }

    public void Dispose()
    {
        this.ID3D11DeviceContext.Dispose();
        GC.SuppressFinalize(this);
    }
}