using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class DepthStencilState : IDisposable
    {
        internal DepthStencilState(ID3D11DepthStencilState state, string name)
        {
            this.ID3D11DepthStencilState = state;
            this.ID3D11DepthStencilState.DebugName = name;
            this.Name = name;
        }

        public string Name { get; }

        internal ID3D11DepthStencilState ID3D11DepthStencilState { get; }

        public void Dispose()
        {
            this.ID3D11DepthStencilState.Dispose();
        }
    }

    public sealed class DepthStencilStates : IDisposable
    {
        internal DepthStencilStates(ID3D11Device device)
        {
            this.None = Create(device, NoneDescription(), nameof(this.None));
            this.Default = Create(device, DefaultDescription(), nameof(this.Default));
        }

        public DepthStencilState None { get; }
        public DepthStencilState Default { get; }

        private static DepthStencilState Create(ID3D11Device device, DepthStencilDescription description, string name)
        {
            var state = device.CreateDepthStencilState(description);
            return new DepthStencilState(state, name);
        }

        private static DepthStencilDescription NoneDescription()
        {
            var stencilOpDesc = new DepthStencilOperationDescription(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, ComparisonFunction.Always);
            var depthDesc = new DepthStencilDescription
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Always,
                StencilEnable = false,
                FrontFace = stencilOpDesc,
                BackFace = stencilOpDesc
            };

            return depthDesc;
        }

        private static DepthStencilDescription DefaultDescription()
        {
            var stencilOpDesc = new DepthStencilOperationDescription(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, ComparisonFunction.Always);
            var depthDesc = new DepthStencilDescription
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Less,
                StencilEnable = false,
                FrontFace = stencilOpDesc,
                BackFace = stencilOpDesc
            };

            return depthDesc;
        }

        public void Dispose()
        {
            this.None.Dispose();
        }
    }
}
