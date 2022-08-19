using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts.States;

public sealed class RasterizerState : IDisposable
{
    internal RasterizerState(ID3D11RasterizerState state, string meaning)
    {
        this.Name = DebugNameGenerator.GetName(nameof(RasterizerState), meaning);

        this.State = state;
        this.State.DebugName = this.Name;
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
        this.WireFrame = Create(device, RasterizerDescription.Wireframe, nameof(this.WireFrame));

        this.CullNone = Create(device, RasterizerDescription.CullNone, nameof(this.CullNone));

        this.CullCounterClockwise = Create(device, RasterizerDescription.CullBack, nameof(this.CullCounterClockwise));
        this.CullNoneCounterClockwiseScissor = Create(device, CreateCullNoneCounterClockwiseScissor(), nameof(this.CullNoneCounterClockwiseScissor));
        this.CullClockwise = Create(device, RasterizerDescription.CullFront, nameof(this.CullClockwise));

        this.CullNoneNoDepthClip = Create(device, CreateCullNoneNoDepthClip(), nameof(this.CullNoneNoDepthClip));
        this.CullCounterClockwiseNoDepthClip = Create(device, CreateCullCounterClockwiseNoDepthClip(), nameof(this.CullCounterClockwiseNoDepthClip));
        this.CullClockwiseNoDepthClip = Create(device, CreateCullClockwiseNoDepthClip(), nameof(this.CullClockwiseNoDepthClip));

        this.Default = this.CullCounterClockwise;
    }

    public RasterizerState Default { get; set; }

    public RasterizerState WireFrame { get; }

    public RasterizerState CullNone { get; }
    public RasterizerState CullNoneCounterClockwiseScissor { get; }
    public RasterizerState CullCounterClockwise { get; }
    public RasterizerState CullClockwise { get; }

    public RasterizerState CullNoneNoDepthClip { get; }
    public RasterizerState CullCounterClockwiseNoDepthClip { get; }
    public RasterizerState CullClockwiseNoDepthClip { get; }

    private static RasterizerState Create(ID3D11Device device, RasterizerDescription description, string name)
    {
        var state = device.CreateRasterizerState(description);
        return new RasterizerState(state, name);
    }

    private static RasterizerDescription CreateCullNoneCounterClockwiseScissor()
    {
        return new RasterizerDescription()
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            FrontCounterClockwise = false,
            DepthBias = 0,
            DepthBiasClamp = 0f,
            SlopeScaledDepthBias = 0f,
            DepthClipEnable = true,
            ScissorEnable = true,
            MultisampleEnable = true,
            AntialiasedLineEnable = false,
        };
    }

    private static RasterizerDescription CreateCullNoneNoDepthClip()
    {
        return new RasterizerDescription()
        {
            CullMode = CullMode.None,
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
