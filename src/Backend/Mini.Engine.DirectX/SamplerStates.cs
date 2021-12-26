using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public sealed class SamplerState : IDisposable
{
    internal SamplerState(ID3D11SamplerState state, string name)
    {
        this.State = state;
        this.State.DebugName = name;
        this.Name = name;
    }

    public string Name { get; }

    internal ID3D11SamplerState State { get; }

    public void Dispose()
    {
        this.State.Dispose();
    }
}

public sealed class SamplerStates : IDisposable
{
    internal SamplerStates(ID3D11Device device)
    {
        this.LinearWrap = Create(device, SamplerDescription.LinearWrap, nameof(this.LinearWrap));
        this.AnisotropicWrap = Create(device, SamplerDescription.AnisotropicWrap, nameof(this.AnisotropicWrap));
    }

    public SamplerState LinearWrap { get; }

    public SamplerState AnisotropicWrap { get; }


    private static SamplerState Create(ID3D11Device device, SamplerDescription description, string name)
    {
        var state = device.CreateSamplerState(description);
        return new SamplerState(state, name);
    }

    public void Dispose()
    {
        this.LinearWrap.Dispose();
    }
}
