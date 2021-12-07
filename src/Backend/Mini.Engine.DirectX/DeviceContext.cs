using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public abstract class DeviceContextPart
{
    protected readonly DeviceContext DeviceContext;
    protected readonly ID3D11DeviceContext ID3D11DeviceContext;

    protected DeviceContextPart(DeviceContext context)
    {
        this.DeviceContext = context;
        this.ID3D11DeviceContext = context.ID3D11DeviceContext;
    }
}

public sealed class InputAssemblerContext : DeviceContextPart
{
    public InputAssemblerContext(DeviceContext context)
        : base(context) { }

    public void SetVertexBuffer<T>(VertexBuffer<T> buffer)
        where T : unmanaged
    {
        var stride = buffer.PrimitiveSizeInBytes;
        var offset = 0;
        this.ID3D11DeviceContext.IASetVertexBuffers(0, 1, new[] { buffer.Buffer }, new[] { stride }, new[] { offset });
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

    public void SetPrimitiveTopology(PrimitiveTopology topology)
    {
        this.ID3D11DeviceContext.IASetPrimitiveTopology(topology);
    }
}

public sealed class VertexShaderContext : DeviceContextPart
{
    public VertexShaderContext(DeviceContext context)
        : base(context) { }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.VSSetConstantBuffer(slot, buffer.Buffer);
    }

    public void SetShader(IVertexShader shader)
    {
        this.ID3D11DeviceContext.VSSetShader(shader.ID3D11Shader);
    }
}

public sealed class PixelShaderContext : DeviceContextPart
{
    public PixelShaderContext(DeviceContext context)
        : base(context) { }

    public void SetSampler(int slot, SamplerState sampler)
    {
        this.ID3D11DeviceContext.PSSetSampler(slot, sampler.State);
    }

    public void SetSamplers(int startSlot, params SamplerState[] samplers)
    {
        var nativeSamplers = new ID3D11SamplerState[samplers.Length];
        for (var i = 0; i < samplers.Length; i++)
        {
            nativeSamplers[i] = samplers[i].State;
        }

        this.ID3D11DeviceContext.PSSetSamplers(startSlot, nativeSamplers);
    }

    public void SetShader(IPixelShader shader)
    {
        this.ID3D11DeviceContext.PSSetShader(shader.ID3D11Shader);
    }

    public void SetShaderResource(int slot, Texture2D texture)
    {
        this.ID3D11DeviceContext.PSSetShaderResource(slot, texture.ShaderResourceView);
    }
}

public sealed class OutputMergerContext : DeviceContextPart
{
    public OutputMergerContext(DeviceContext context)
        : base(context) { }

    public void SetBlendState(BlendState state)
    {
        this.ID3D11DeviceContext.OMSetBlendState(state.ID3D11BlendState);
    }

    public void SetDepthStencilState(DepthStencilState state)
    {
        this.ID3D11DeviceContext.OMSetDepthStencilState(state.ID3D11DepthStencilState);
    }

    public void SetRenderTargetToBackBuffer(DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(base.DeviceContext.Device.BackBufferView, depthStencilBuffer?.DepthStencilView);
    }

    public void SetRenderTarget(RenderTarget2D renderTarget, DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetView, depthStencilBuffer?.DepthStencilView);
    }
}

public sealed class RasterizerContext : DeviceContextPart
{
    public RasterizerContext(DeviceContext context)
        : base(context) { }

    public void SetRasterizerState(RasterizerState state)
    {
        this.ID3D11DeviceContext.RSSetState(state.State);
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
        this.ID3D11DeviceContext.RSSetScissorRect(x, y, width, height);
    }

    public void SetViewPort(int x, int y, float width, float height)
    {
        this.ID3D11DeviceContext.RSSetViewport(x, y, width, height);
    }
}

public abstract class DeviceContext : IDisposable
{
    internal DeviceContext(Device device, ID3D11DeviceContext context, string name)
    {
        this.Device = device;
        this.ID3D11DeviceContext = context;
        this.ID3D11DeviceContext.SetName(name);

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

public sealed class ImmediateDeviceContext : DeviceContext
{
    public ImmediateDeviceContext(Device device, ID3D11DeviceContext context, string name)
        : base(device, context, name) { }

    public void ExecuteCommandList(CommandList commandList)
    {
        this.ID3D11DeviceContext.ExecuteCommandList(commandList.ID3D11CommandList, false);
    }
}

public sealed class DeferredDeviceContext : DeviceContext
{
    public DeferredDeviceContext(Device device, ID3D11DeviceContext context, string name)
        : base(device, context, name) { }

    public CommandList FinishCommandList()
    {
        return new(this.ID3D11DeviceContext.FinishCommandList(false));
    }
}
