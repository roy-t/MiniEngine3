using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Contexts;

public abstract class DeviceContext : IDisposable
{
    internal DeviceContext(Device device, ID3D11DeviceContext context, string user, string meaning)
    {
        this.Device = device;
        this.ID3D11DeviceContext = context;
        this.Resources = device.Resources;
        this.ID3D11DeviceContext.DebugName = DebugNameGenerator.GetName(user, user, meaning);

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

    public Device Device { get; }
    public LifetimeManager Resources { get; }

    internal ID3D11DeviceContext ID3D11DeviceContext { get; }


    public T[] GetSurfaceData<T>(RWTexture source, int mipSlice = 0, int arraySlice = 0)
        where T : unmanaged
    {        
        var data = new T[source.ImageInfo.Pixels];
        this.CopySurfaceDataToSpan<T>(source, data, mipSlice, arraySlice);

        return data;
    }

    public void CopySurfaceDataToSpan<T>(ISurface source, Span<T> output, int mipSlice = 0, int arraySlice = 0)
        where T : unmanaged
    {
        var ctx = this.ID3D11DeviceContext;

        using var staging = new StagingBuffer<T>(this.Device, source.ImageInfo, source.MipMapInfo, "staging_copy");
        ctx.CopyResource(staging.Buffer, source.Texture);
        var resource = ctx.Map(staging.Buffer, mipSlice, arraySlice, MapMode.Read, MapFlags.None, out var subresource, out int mipsize);
        ctx.Flush();
        
        var span = resource.AsSpan<T>(staging.Buffer, mipSlice, arraySlice);
        span.CopyTo(output);

        ctx.Unmap(staging.Buffer, mipSlice, arraySlice);
    }


    public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
    {
        this.ID3D11DeviceContext.DrawIndexed(indexCount, indexOffset, vertexOffset);
    }

    public void Draw(int vertexCount, int startVertexLocation = 0)
    {
        this.ID3D11DeviceContext.Draw(vertexCount, startVertexLocation);
    }

    public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation = 0, int startInstanceLocation = 0)
    {
        this.ID3D11DeviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
    }

    public void Clear(IRWTexture texture, Color4 color)
    {
        this.ID3D11DeviceContext.ClearUnorderedAccessView(texture.UnorderedAccessViews[0], color);
    }

    public void Clear(IRenderTarget renderTarget, Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(renderTarget.ID3D11RenderTargetViews[0], color);
    }

    public void Clear(IRenderTarget renderTarget, int slice, Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(renderTarget.ID3D11RenderTargetViews[slice], color);
    }

    public void Clear(IDepthStencilBuffer depthStencilBuffer, DepthStencilClearFlags flags, float depth, byte stencil)
    {
        this.ID3D11DeviceContext.ClearDepthStencilView(depthStencilBuffer.DepthStencilViews[0], flags, depth, stencil);
    }

    public void Clear(IDepthStencilBuffer depthStencilBuffers, int slice, DepthStencilClearFlags flags, float depth, byte stencil)
    {
        this.ID3D11DeviceContext.ClearDepthStencilView(depthStencilBuffers.DepthStencilViews[slice], flags, depth, stencil);
    }

    public void Clear(ILifetime<IDepthStencilBuffer> depthStencilBuffers, int slice, DepthStencilClearFlags flags, float depth, byte stencil)
    {
        var dsv = this.Resources.Get(depthStencilBuffers).DepthStencilViews[slice];
        this.ID3D11DeviceContext.ClearDepthStencilView(dsv, flags, depth, stencil);
    }

    public void ClearBackBuffer()
    {
        this.ClearBackBuffer(new Color4(0, 0, 0));
    }

    public void ClearBackBuffer(Color4 color)
    {
        this.ID3D11DeviceContext.ClearRenderTargetView(this.Device.BackBufferView, color);
    }

    public void Setup(InputLayout? inputLayout, ILifetime<IVertexShader> vertex, ILifetime<IPixelShader> pixel, BlendState blend, DepthStencilState depth)
    {
        this.Setup
        (
            inputLayout,
            PrimitiveTopology.TriangleList,
            vertex,
            this.Device.RasterizerStates.Default,
            0,
            0,
            this.Device.Width,
            this.Device.Height,
            pixel,
            blend,
            depth
        );
    }

    public void Setup(InputLayout? inputLayout, ILifetime<IVertexShader> vertex, int width, int height, ILifetime<IPixelShader> pixel, BlendState blend, DepthStencilState depth)
    {
        this.Setup
        (
            inputLayout,
            PrimitiveTopology.TriangleList,
            vertex,
            this.Device.RasterizerStates.Default,
            0,
            0,
            width,
            height,
            pixel,
            blend,
            depth
        );
    }

    public void Setup(InputLayout? layout, PrimitiveTopology primitive, ILifetime<IVertexShader> vertex, RasterizerState rasterizer, int x, int y, int width, int height, ILifetime<IPixelShader> pixel, BlendState blend, DepthStencilState depth)
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

    public void SetupFullScreenTriangle(ILifetime<IVertexShader> vertex, ILifetime<IPixelShader> pixel, BlendState blend, DepthStencilState depth)
    {
        this.SetupFullScreenTriangle(vertex, 0, 0, this.Device.Width, this.Device.Height, pixel, blend, depth);
    }

    public void SetupFullScreenTriangle(ILifetime<IVertexShader> vertex, int width, int height, ILifetime<IPixelShader> pixel, BlendState blend, DepthStencilState depth)
    {
        this.SetupFullScreenTriangle(vertex, 0, 0, width, height, pixel, blend, depth);
    }

    public void SetupFullScreenTriangle(ILifetime<IVertexShader> vertex, int x, int y, int width, int height, ILifetime<IPixelShader> pixel, BlendState blend, DepthStencilState depth)
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

    public void Dispose()
    {
        this.ID3D11DeviceContext.Dispose();
        GC.SuppressFinalize(this);
    }
}