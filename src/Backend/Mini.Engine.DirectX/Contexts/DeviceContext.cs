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


    public void Setup(InputLayout inputLayout, IVertexShader vertex, IPixelShader pixel, BlendState blend, DepthStencilState depth)
    {
        this.Setup
        (
            inputLayout,
            PrimitiveTopology.TriangleList,
            vertex,
            this.Device.RasterizerStates.CullCounterClockwise,
            0,
            0,
            this.Device.Width,
            this.Device.Height,
            pixel,
            blend,
            depth
        );
    }

    public void Setup(InputLayout inputLayout, IVertexShader vertex, IPixelShader pixel, BlendState blend, DepthStencilState depth, int width, int height)
    {
        this.Setup
        (
            inputLayout,
            PrimitiveTopology.TriangleList,
            vertex,
            this.Device.RasterizerStates.CullCounterClockwise,
            0,
            0,
            width,
            height,
            pixel,
            blend,
            depth
        );
    }

    public void Setup(InputLayout layout, PrimitiveTopology primitive, IVertexShader vertex, RasterizerState rasterizer, int x, int y, int width, int height, IPixelShader pixel, BlendState blend, DepthStencilState depth)
    {
        this.IA.SetInputLayout(layout);
        this.IA.SetPrimitiveTopology(primitive);

        this.VS.SetShader(vertex);

        this.RS.SetRasterizerState(rasterizer);
        this.RS.SetScissorRect(x, y, width, height);
        this.RS.SetViewPort(x, y, width, height);

        this.PS.SetShader(pixel);

        this.OM.SetBlendState(blend);
        this.OM.SetDepthStencilState(depth);
    }

    public Device Device { get; }
    internal ID3D11DeviceContext ID3D11DeviceContext { get; }

    public void Dispose()
    {
        this.ID3D11DeviceContext.Dispose();
        GC.SuppressFinalize(this);
    }
}