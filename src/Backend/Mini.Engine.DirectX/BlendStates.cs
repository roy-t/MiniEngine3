using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public sealed class BlendState : IDisposable
{
    internal BlendState(ID3D11BlendState state, string name)
    {
        this.ID3D11BlendState = state;
        this.ID3D11BlendState.DebugName = name;
        this.Name = name;
    }

    public string Name { get; }

    internal ID3D11BlendState ID3D11BlendState { get; }

    public void Dispose()
    {
        this.ID3D11BlendState.Dispose();
    }
}

public sealed class BlendStates : IDisposable
{
    internal BlendStates(ID3D11Device device)
    {
        this.AlphaBlend = Create(device, AlphaBlendDescription(), nameof(this.AlphaBlend));
        this.Opaque = Create(device, OpaqueBlendDescription(), nameof(this.Opaque));
    }

    public BlendState AlphaBlend { get; }
    public BlendState Opaque { get; }

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

    private static BlendDescription OpaqueBlendDescription()
    {
        var blendDesc = new BlendDescription
        {
            AlphaToCoverageEnable = false
        };

        blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
        {
            IsBlendEnabled = false,
            RenderTargetWriteMask = ColorWriteEnable.All
        };

        return blendDesc;
    }

    public void Dispose()
    {
        this.AlphaBlend.Dispose();
        this.Opaque.Dispose();
    }
}
