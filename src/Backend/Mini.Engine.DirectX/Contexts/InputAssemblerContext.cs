using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D;

namespace Mini.Engine.DirectX.Contexts;

public sealed class InputAssemblerContext : DeviceContextPart
{
    public InputAssemblerContext(DeviceContext context)
        : base(context) { }

    public void SetVertexBuffer<T>(VertexBuffer<T> buffer, int vertexOffset = 0)
        where T : unmanaged
    {
        var stride = buffer.PrimitiveSizeInBytes;
        var offset = vertexOffset * stride;
        this.ID3D11DeviceContext.IASetVertexBuffer(0, buffer.Buffer, stride, offset);
    }

    public void SetIndexBuffer<T>(IndexBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.IASetIndexBuffer(buffer.Buffer, buffer.Format, 0);
    }

    public void SetInputLayout(InputLayout inputLayout)
    {
        this.ID3D11DeviceContext.IASetInputLayout(inputLayout.ID3D11InputLayout);
    }

    public void ClearInputLayout()
    {
        this.ID3D11DeviceContext.IASetInputLayout(null);
    }

    public void SetPrimitiveTopology(PrimitiveTopology topology)
    {
        this.ID3D11DeviceContext.IASetPrimitiveTopology(topology);
    }
}
