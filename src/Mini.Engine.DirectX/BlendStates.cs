using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class BlendState
    {
        internal BlendState(ID3D11BlendState state, string name)
        {
            this.State = state;
            this.State.DebugName = name;
            this.Name = name;
        }

        public string Name { get; }

        internal ID3D11BlendState State { get; }
    }

    public sealed class BlendStates
    {
        internal BlendStates(ID3D11Device device)
        {
            this.AlphaBlend = Create(device, AlphaBlendDescription(), nameof(this.AlphaBlend));
        }

        public BlendState AlphaBlend { get; }

        private static BlendState Create(ID3D11Device device, BlendDescription description, string name)
        {
            var state = device.CreateBlendState(description);
            return new BlendState(state, name);
        }

        private static BlendDescription AlphaBlendDescription()
        {
            var blendDesc = new BlendDescription
            {
                AlphaToCoverageEnable = false
            };

            blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = Blend.SourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.InverseSourceAlpha,
                DestinationBlendAlpha = Blend.Zero,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All
            };

            return blendDesc;
        }
    }
}
