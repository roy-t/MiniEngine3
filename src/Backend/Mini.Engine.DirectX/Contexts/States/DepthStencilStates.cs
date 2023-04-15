using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts.States;

public sealed class DepthStencilState : IDisposable
{
    internal DepthStencilState(ID3D11DepthStencilState state, string meaning)
    {
        this.Name = DebugNameGenerator.GetName(nameof(DepthStencilState), meaning);

        this.ID3D11DepthStencilState = state;
        this.ID3D11DepthStencilState.DebugName = this.Name;

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
        this.None = Create(device, DepthStencilDescription.None, nameof(this.None));
        this.Default = Create(device, DepthStencilDescription.Default, nameof(this.Default));
        this.ReverseZ = Create(device, DepthStencilDescription.DepthReverseZ, nameof(this.ReverseZ));
        this.ReverseZReadOnly = Create(device, DepthStencilDescription.DepthReadReverseZ, nameof(this.ReverseZReadOnly));
    }

    public DepthStencilState None { get; }
    public DepthStencilState Default { get; }
    public DepthStencilState ReverseZ { get; }
    public DepthStencilState ReverseZReadOnly { get; }

    private static DepthStencilState Create(ID3D11Device device, DepthStencilDescription description, string name)
    {
        var state = device.CreateDepthStencilState(description);
        return new DepthStencilState(state, name);
    }

    public void Dispose()
    {
        this.None.Dispose();
        this.Default.Dispose();
        this.ReverseZ.Dispose();
        this.ReverseZReadOnly.Dispose();
    }
}
