using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts.States;

public sealed class RasterizerState : IDisposable
{
    internal RasterizerState(ID3D11RasterizerState state, string meaning, RasterizerDescription description)
    {
        this.Name = DebugNameGenerator.GetName(nameof(RasterizerState), meaning);

        this.State = state;
        this.State.DebugName = this.Name;
        this.Description = description;
    }

    public string Name { get; }

    internal RasterizerDescription Description { get; }
    internal ID3D11RasterizerState State { get; }

    public void Dispose()
    {
        this.State.Dispose();
    }
}

public sealed class RasterizerStates : IDisposable
{
    private const int DefaultDepthBias = 0;
    private const float DefaultDepthBiasClamp = 0.0f;
    private const float DefaultSlopeScaledDepthBias = 0.0f;

    internal RasterizerStates(ID3D11Device device)
    {
        // TODO: default descriptions do not enable the scissor rectangle!

        this.WireFrame = Create(device, RasterizerDescription.Wireframe, nameof(this.WireFrame));
        this.Line = Create(device, CreateLine(), nameof(this.Line));

        this.CullNone = Create(device, RasterizerDescription.CullNone, nameof(this.CullNone));

        this.CullCounterClockwise = Create(device, RasterizerDescription.CullBack, nameof(this.CullCounterClockwise));
        this.CullClockwise = Create(device, RasterizerDescription.CullFront, nameof(this.CullClockwise));

        this.CullNoneNoDepthClip = Create(device, CreateCullNoneNoDepthClip(), nameof(this.CullNoneNoDepthClip));
        this.CullCounterClockwiseNoDepthClip = Create(device, CreateCullCounterClockwiseNoDepthClip(), nameof(this.CullCounterClockwiseNoDepthClip));
        this.CullClockwiseNoDepthClip = Create(device, CreateCullClockwiseNoDepthClip(), nameof(this.CullClockwiseNoDepthClip));

        this.Default = this.CullCounterClockwise;
    }

    public RasterizerState Default { get; set; }

    public RasterizerState WireFrame { get; }

    /// <summary>
    /// Note: incompatible with MSAA
    /// </summary>
    public RasterizerState Line { get; }

    public RasterizerState CullNone { get; }

    public RasterizerState CullCounterClockwise { get; }

    public RasterizerState CullClockwise { get; }

    public RasterizerState CullNoneNoDepthClip { get; }
    public RasterizerState CullCounterClockwiseNoDepthClip { get; }
    public RasterizerState CullClockwiseNoDepthClip { get; }

    public static RasterizerState CreateBiased(Device device, RasterizerState template, int depthBias, float depthBiasClamp = 0.0f, float slopeScaledDepthBias = 0.0f)
    {
        var description = template.Description;
        description.DepthBias = depthBias;
        description.DepthBiasClamp = depthBiasClamp;
        description.SlopeScaledDepthBias = slopeScaledDepthBias;
        return Create(device.ID3D11Device, description, string.Empty);
    }

    private static RasterizerState Create(ID3D11Device device, RasterizerDescription description, string name)
    {
        description.ScissorEnable = true;
        var state = device.CreateRasterizerState(description);
        return new RasterizerState(state, name, description);
    }

    private static RasterizerDescription CreateCullNoneNoDepthClip()
    {
        return new RasterizerDescription()
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            FrontCounterClockwise = false,
            DepthBias = DefaultDepthBias,
            DepthBiasClamp = DefaultDepthBiasClamp,
            SlopeScaledDepthBias = DefaultSlopeScaledDepthBias,
            DepthClipEnable = false,
            ScissorEnable = true,
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
            DepthBias = DefaultDepthBias,
            DepthBiasClamp = DefaultDepthBiasClamp,
            SlopeScaledDepthBias = DefaultSlopeScaledDepthBias,
            DepthClipEnable = false,
            ScissorEnable = true,
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
            DepthBias = DefaultDepthBias,
            DepthBiasClamp = DefaultDepthBiasClamp,
            SlopeScaledDepthBias = DefaultSlopeScaledDepthBias,
            DepthClipEnable = false,
            ScissorEnable = true,
            MultisampleEnable = true,
            AntialiasedLineEnable = false,
        };
    }

    private static RasterizerDescription CreateLine()
    {

        return new RasterizerDescription()
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            FrontCounterClockwise = false,
            DepthBias = DefaultDepthBias,
            DepthBiasClamp = DefaultDepthBiasClamp,
            SlopeScaledDepthBias = DefaultSlopeScaledDepthBias,
            DepthClipEnable = true,
            ScissorEnable = true,
            MultisampleEnable = false,
            AntialiasedLineEnable = true,
        };
    }

    public void Dispose()
    {
        this.Default.Dispose();
        this.WireFrame.Dispose();
        this.Line.Dispose();

        this.CullNone.Dispose();
        this.CullCounterClockwise.Dispose();
        this.CullClockwise.Dispose();

        this.CullNoneNoDepthClip.Dispose();
        this.CullCounterClockwiseNoDepthClip.Dispose();
        this.CullClockwiseNoDepthClip.Dispose();
    }
}
