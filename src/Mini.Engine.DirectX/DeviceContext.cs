using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public abstract class DeviceContextPart
    {
        protected readonly ID3D11DeviceContext ID3D11DeviceContext;

        protected DeviceContextPart(ID3D11DeviceContext iD3D11DeviceContext)
        {
            this.ID3D11DeviceContext = iD3D11DeviceContext;
        }
    }

    public sealed class InputAssemblerContext : DeviceContextPart
    {
        public InputAssemblerContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetVertexBuffer<T>(VertexBuffer<T> buffer)
            where T : unmanaged
        {
            var stride = buffer.PrimitiveSizeInBytes;
            var offset = 0;
            this.ID3D11DeviceContext.IASetVertexBuffers(0, 1, new[] { buffer.Buffer }, new[] { stride }, new[] { offset });
        }

        public void SetIndexBuffer<T>(IndexBuffer<T> buffer)
            where T : unmanaged
            => this.ID3D11DeviceContext.IASetIndexBuffer(buffer.Buffer, buffer.Format, 0);

        public void SetInputLayout(InputLayout inputLayout)
            => this.ID3D11DeviceContext.IASetInputLayout(inputLayout.ID3D11InputLayout);

        public void SetPrimitiveTopology(PrimitiveTopology topology)
            => this.ID3D11DeviceContext.IASetPrimitiveTopology(topology);
    }

    public sealed class VertexShaderContext : DeviceContextPart
    {
        public VertexShaderContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
            where T : unmanaged
            => this.ID3D11DeviceContext.VSSetConstantBuffer(slot, buffer.Buffer);
    }

    public sealed class PixelShaderContext : DeviceContextPart
    {
        public PixelShaderContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetSampler(int slot, SamplerState sampler)
            => this.ID3D11DeviceContext.PSSetSampler(slot, sampler.State);

        public void SetSamplers(int startSlot, params SamplerState[] samplers)
        {
            var nativeSamplers = new ID3D11SamplerState[samplers.Length];
            for (var i = 0; i < samplers.Length; i++)
            {
                nativeSamplers[i] = samplers[i].State;
            }

            this.ID3D11DeviceContext.PSSetSamplers(startSlot, nativeSamplers);
        }

        public void SetShaderResource(int slot, Texture2D texture)
            => this.ID3D11DeviceContext.PSSetShaderResource(slot, texture.ShaderResourceView);
    }

    public sealed class OutputMergerContext : DeviceContextPart
    {
        public OutputMergerContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetBlendState(BlendState state)
            => this.ID3D11DeviceContext.OMSetBlendState(state.ID3D11BlendState);

        public void SetDepthStencilState(DepthStencilState state)
            => this.ID3D11DeviceContext.OMSetDepthStencilState(state.ID3D11DepthStencilState);

        public void SetRenderTarget(RenderTarget2D renderTarget)
            => this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetView);
    }

    public sealed class RasterizerContext : DeviceContextPart
    {
        public RasterizerContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetState(RasterizerState state)
            => this.ID3D11DeviceContext.RSSetState(state.State);

        public void SetScissorRect(int x, int y, int width, int height)
            => this.ID3D11DeviceContext.RSSetScissorRect(x, y, width, height);

        public void SetViewPort(int x, int y, float width, float height)
            => this.ID3D11DeviceContext.RSSetViewport(x, y, width, height);
    }

    public abstract class DeviceContext : IDisposable
    {
        internal DeviceContext(ID3D11DeviceContext context)
        {
            this.IA = new InputAssemblerContext(context);
            this.VS = new VertexShaderContext(context);
            this.PS = new PixelShaderContext(context);
            this.OM = new OutputMergerContext(context);
            this.RS = new RasterizerContext(context);

            this.ID3D11DeviceContext = context;
        }

        public InputAssemblerContext IA { get; }
        public VertexShaderContext VS { get; }
        public PixelShaderContext PS { get; }
        public OutputMergerContext OM { get; }
        public RasterizerContext RS { get; }

        public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
            => this.ID3D11DeviceContext.DrawIndexed(indexCount, indexOffset, vertexOffset);

        internal ID3D11DeviceContext ID3D11DeviceContext { get; }

        public void Dispose()
            => this.ID3D11DeviceContext.Dispose();
    }

    public sealed class ImmediateDeviceContext : DeviceContext
    {
        public ImmediateDeviceContext(ID3D11DeviceContext context)
            : base(context) { }

        public void ExecuteCommandList(CommandList commandList)
           => this.ID3D11DeviceContext.ExecuteCommandList(commandList.ID3D11CommandList, false);
    }

    public sealed class DeferredDeviceContext : DeviceContext
    {
        public DeferredDeviceContext(ID3D11DeviceContext context)
            : base(context) { }

        public CommandList FinishCommandList()
           => new(this.ID3D11DeviceContext.FinishCommandList(false));
    }
}
