using System;
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
    }

    public sealed class VertexShaderContext : DeviceContextPart
    {
        public VertexShaderContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }
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
    }

    public sealed class OutputMergerContext : DeviceContextPart
    {
        public OutputMergerContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetBlendState(BlendState state)
            => this.ID3D11DeviceContext.OMSetBlendState(state.State);

        public void SetDepthStencilState(DepthStencilState state)
            => this.ID3D11DeviceContext.OMSetDepthStencilState(state.State);
    }

    public sealed class RasterizerContext : DeviceContextPart
    {
        public RasterizerContext(ID3D11DeviceContext iD3D11DeviceContext)
            : base(iD3D11DeviceContext) { }

        public void SetState(RasterizerState state)
            => this.ID3D11DeviceContext.RSSetState(state.State);
    }

    public sealed class DeviceContext : IDisposable
    {
        private readonly ID3D11DeviceContext Context;

        internal DeviceContext(ID3D11DeviceContext context)
        {
            this.IA = new InputAssemblerContext(context);
            this.VS = new VertexShaderContext(context);
            this.PS = new PixelShaderContext(context);
            this.OM = new OutputMergerContext(context);
            this.RS = new RasterizerContext(context);

            this.Context = context;
        }

        public InputAssemblerContext IA { get; }
        public VertexShaderContext VS { get; }
        public PixelShaderContext PS { get; }
        public OutputMergerContext OM { get; }
        public RasterizerContext RS { get; }


        // TODO: temp
        public ID3D11DeviceContext GetContext() => this.Context;

        public void Dispose()
            => this.Context.Release();
    }
}
