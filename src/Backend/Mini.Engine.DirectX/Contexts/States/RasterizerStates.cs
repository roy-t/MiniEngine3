using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts.States;

public sealed class RasterizerState : IDisposable
{
    internal RasterizerState(ID3D11RasterizerState state, string name)
    {
        this.State = state;
        this.State.DebugName = name;
        this.Name = name;
    }

    public string Name { get; }

    internal ID3D11RasterizerState State { get; }

    public void Dispose()
    {
        this.State.Dispose();
    }
}

public sealed class RasterizerStates : IDisposable
{
    internal RasterizerStates(ID3D11Device device)
    {
        this.CullNone = Create(device, RasterizerDescription.CullNone, nameof(this.CullNone));
        this.CullCounterClockwise = Create(device, RasterizerDescription.CullCounterClockwise, nameof(this.CullCounterClockwise));
        this.CullClockwise = Create(device, RasterizerDescription.CullClockwise, nameof(this.CullClockwise));
    }

    public RasterizerState CullNone { get; }
    public RasterizerState CullCounterClockwise { get; }
    public RasterizerState CullClockwise { get; }

    private static RasterizerState Create(ID3D11Device device, RasterizerDescription description, string name)
    {
        var state = device.CreateRasterizerState(description);
        return new RasterizerState(state, name);
    }

    public void Dispose()
    {
        this.CullNone.Dispose();
        this.CullCounterClockwise.Dispose();
        this.CullClockwise.Dispose();
    }
}
