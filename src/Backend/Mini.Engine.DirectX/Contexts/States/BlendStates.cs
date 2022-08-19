using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts.States;

public sealed class BlendState : IDisposable
{
    internal BlendState(ID3D11BlendState state, string meaning)
    {
        this.Name = DebugNameGenerator.GetName(nameof(BlendState), meaning);

        this.ID3D11BlendState = state;
        this.ID3D11BlendState.DebugName = this.Name;
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
        this.NonPreMultiplied = Create(device, BlendDescription.NonPremultiplied, nameof(this.NonPreMultiplied));
        this.AlphaBlend = Create(device, BlendDescription.AlphaBlend, nameof(this.AlphaBlend));
        this.Opaque = Create(device, BlendDescription.Opaque, nameof(this.Opaque));
        this.Additive = Create(device, BlendDescription.Additive, nameof(this.Additive));
    }

    public BlendState NonPreMultiplied { get; }
    public BlendState AlphaBlend { get; }
    public BlendState Opaque { get; }
    public BlendState Additive { get; }

    private static BlendState Create(ID3D11Device device, BlendDescription description, string name)
    {
        var state = device.CreateBlendState(description);
        return new BlendState(state, name);
    }

    public void Dispose()
    {
        this.NonPreMultiplied.Dispose();
        this.AlphaBlend.Dispose();
        this.Opaque.Dispose();
        this.Additive.Dispose();
    }
}
