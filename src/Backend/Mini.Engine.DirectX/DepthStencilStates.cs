using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

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
        this.None = Create(device, DepthStencilDescription.None, nameof(this.None));
        this.Default = Create(device, DepthStencilDescription.Default, nameof(this.Default));
        this.ReadOnly = Create(device, DepthStencilDescription.DepthRead, nameof(this.ReadOnly));
    }

    public DepthStencilState None { get; }
    public DepthStencilState Default { get; }
    public DepthStencilState ReadOnly { get; }

    private static DepthStencilState Create(ID3D11Device device, DepthStencilDescription description, string name)
    {
        var state = device.CreateDepthStencilState(description);
        return new DepthStencilState(state, name);
    }

    public void Dispose()
    {
        this.None.Dispose();
        this.Default.Dispose();
        this.ReadOnly.Dispose();
    }
}
