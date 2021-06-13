using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class DepthStencilState
    {
        internal DepthStencilState(ID3D11DepthStencilState state, string name)
        {
            this.State = state;
            this.State.DebugName = name;
            this.Name = name;
        }

        public string Name { get; }

        internal ID3D11DepthStencilState State { get; }
    }

    public sealed class DepthStencilStates
    {
        internal DepthStencilStates(ID3D11Device device)
        {
            this.None = Create(device, NoneDescription(), nameof(this.None));
        }

        public DepthStencilState None { get; }

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
    }
}
