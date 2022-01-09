using System;
using System.IO;
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
        this.CullCounterClockwise = Create(device, RasterizerDescription.Cullback, nameof(this.CullCounterClockwise));
        this.CullClockwise = Create(device, RasterizerDescription.CullFront, nameof(this.CullClockwise));

        this.CullCounterClockwiseNoDepthClip = Create(device, CreateCullCounterClockwiseNoDepthClip(), nameof(this.CullCounterClockwiseNoDepthClip));
        this.CullClockwiseNoDepthClip = Create(device, CreateCullClockwiseNoDepthClip(), nameof(this.CullClockwiseNoDepthClip));
    }

    public RasterizerState CullNone { get; }
    public RasterizerState CullCounterClockwise { get; }
    public RasterizerState CullClockwise { get; }

    public RasterizerState CullCounterClockwiseNoDepthClip { get; }
    public RasterizerState CullClockwiseNoDepthClip { get; }

    private static RasterizerState Create(ID3D11Device device, RasterizerDescription description, string name)
    {
        var state = device.CreateRasterizerState(description);
        return new RasterizerState(state, name);
    }

    private static RasterizerDescription CreateCullCounterClockwiseNoDepthClip()
    {
        return new RasterizerDescription()
        {
            CullMode = CullMode.Front,
            FillMode = FillMode.Solid,
            FrontCounterClockwise = false,
            DepthBias = 0,
            DepthBiasClamp = 0f,
            SlopeScaledDepthBias = 0f,
            DepthClipEnable = false,
            ScissorEnable = false,
            MultisampleEnable = true,
            AntialiasedLineEnable = false,
        };
    }

    private static RasterizerDescription CreateCullClockwiseNoDepthClip()
    {
        return new RasterizerDescription()
        {
            CullMode = CullMode.Back,
            FillMode = FillMode.Solid,
            FrontCounterClockwise = false,
            DepthBias = 0,
            DepthBiasClamp = 0f,
            SlopeScaledDepthBias = 0f,
            DepthClipEnable = false,
            ScissorEnable = false,
            MultisampleEnable = true,
            AntialiasedLineEnable = false,
        };
    }

    public void Dispose()
    {
        this.CullNone.Dispose();
        this.CullCounterClockwise.Dispose();
        this.CullClockwise.Dispose();

        this.CullCounterClockwiseNoDepthClip.Dispose();
        this.CullClockwiseNoDepthClip.Dispose();
    }
}
