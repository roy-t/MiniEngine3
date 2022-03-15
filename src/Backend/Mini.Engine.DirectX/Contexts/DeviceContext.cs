using System;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

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
        this.CS = new ComputeShaderContext(this);
    }

    public InputAssemblerContext IA { get; }
    public VertexShaderContext VS { get; }
    public RasterizerContext RS { get; }
    public PixelShaderContext PS { get; }
    public OutputMergerContext OM { get; }
    public ComputeShaderContext CS { get; }

    public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
    {
        this.ID3D11DeviceContext.DrawIndexed(indexCount, indexOffset, vertexOffset);
    }

    public void Draw(int vertexCount, int startVertexLocation = 0)
    {
        this.ID3D11DeviceContext.Draw(vertexCount, startVertexLocation);
    }

    public void Clear(RenderTarget2D renderTarget, Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(renderTarget.ID3D11RenderTargetView, color);
    }

    public void Clear(RenderTarget2DArray renderTarget, int slice, Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(renderTarget.ID3D11RenderTargetViews[slice], color);
    }

    public void Clear(RenderTargetCube renderTarget, CubeMapFace face, Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(renderTarget.FaceRenderTargetViews[(int)face], color);
    }

    public void Clear(DepthStencilBuffer depthStencilBuffer, DepthStencilClearFlags flags, float depth, byte stencil)
    {
        this.ID3D11DeviceContext.ClearDepthStencilView(depthStencilBuffer.DepthStencilView, flags, depth, stencil);
    }

    public void Clear(DepthStencilBufferArray depthStencilBuffers, int slice, DepthStencilClearFlags flags, float depth, byte stencil)
    {
        this.ID3D11DeviceContext.ClearDepthStencilView(depthStencilBuffers.DepthStencilViews[slice], flags, depth, stencil);
    }

    public void ClearBackBuffer()
    {
        this.ClearBackBuffer(new Color4(0, 0, 0));
    }

    public void ClearBackBuffer(Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(this.Device.BackBufferView, color);
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

    public void Setup(InputLayout inputLayout, IVertexShader vertex, int width, int height, IPixelShader pixel, BlendState blend, DepthStencilState depth)
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

    public void SetupFullScreenTriangle(IVertexShader vertex, IPixelShader pixel, BlendState blend, DepthStencilState depth)
    {
        this.SetupFullScreenTriangle(vertex, 0, 0, this.Device.Width, this.Device.Height, pixel, blend, depth);
    }

    public void SetupFullScreenTriangle(IVertexShader vertex, int width, int height, IPixelShader pixel, BlendState blend, DepthStencilState depth)
    {
        this.SetupFullScreenTriangle(vertex, 0, 0, width, height, pixel, blend, depth);
    }

    public void SetupFullScreenTriangle(IVertexShader vertex, int x, int y, int width, int height, IPixelShader pixel, BlendState blend, DepthStencilState depth)
    {
        this.IA.ClearInputLayout();
        this.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        this.VS.SetShader(vertex);

        this.RS.SetRasterizerState(this.Device.RasterizerStates.CullNone);
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