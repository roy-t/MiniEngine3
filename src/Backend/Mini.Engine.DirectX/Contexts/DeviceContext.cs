using System;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D;
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
        this.RS = new RasterizerContext(this);
        this.PS = new PixelShaderContext(this);
        this.OM = new OutputMergerContext(this);        
    }

    public InputAssemblerContext IA { get; }
    public VertexShaderContext VS { get; }
    public RasterizerContext RS { get; }
    public PixelShaderContext PS { get; }
    public OutputMergerContext OM { get; }
    
    public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
    {
        this.ID3D11DeviceContext.DrawIndexed(indexCount, indexOffset, vertexOffset);
    }

    public void Setup(InputLayout inputLayout, IVertexShader vertexShader, IPixelShader pixelShader, BlendState blendState, DepthStencilState depthState, int width, int height)
    {
        this.IA.SetInputLayout(inputLayout);
        this.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        this.VS.SetShader(vertexShader);

        this.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);
        this.RS.SetScissorRect(0, 0, width, height);
        this.RS.SetViewPort(0, 0, width, height);

        this.PS.SetShader(pixelShader);

        this.OM.SetBlendState(blendState);
        this.OM.SetDepthStencilState(depthState);        
    }

    public Device Device { get; }
    internal ID3D11DeviceContext ID3D11DeviceContext { get; }

    public void Dispose()
    {
        this.ID3D11DeviceContext.Dispose();
        GC.SuppressFinalize(this);
    }
}