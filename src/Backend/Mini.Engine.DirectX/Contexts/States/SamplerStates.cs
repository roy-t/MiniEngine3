using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Mini.Engine.DirectX.Contexts.States;

public sealed class SamplerState : IDisposable
{
    internal SamplerState(ID3D11SamplerState state, string meaning)
    {
        this.Name = DebugNameGenerator.GetName(nameof(SamplerState), meaning);

        this.State = state;
        this.State.DebugName = this.Name;
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
        this.PointWrap = Create(device, SamplerDescription.PointWrap, nameof(this.PointWrap));
        this.PointClamp = Create(device, SamplerDescription.PointClamp, nameof(this.PointClamp));
        this.LinearWrap = Create(device, SamplerDescription.LinearWrap, nameof(this.LinearWrap));
        this.LinearClamp = Create(device, SamplerDescription.LinearClamp, nameof(this.LinearClamp));
        this.AnisotropicWrap = Create(device, SamplerDescription.AnisotropicWrap, nameof(this.AnisotropicWrap));
        this.CompareLessEqualClamp = Create(device, CreateCompareLessEqualClamp(), nameof(this.CompareLessEqualClamp));
        this.CompareGreaterClamp = Create(device, CreateCompareGreaterClamp(), nameof(this.CompareGreaterClamp));
    }

    public SamplerState PointWrap { get; }
    public SamplerState PointClamp { get; }
    public SamplerState LinearWrap { get; }
    public SamplerState LinearClamp { get; }
    public SamplerState AnisotropicWrap { get; }

    public SamplerState CompareLessEqualClamp { get; }
    public SamplerState CompareGreaterClamp { get; }

    private static SamplerState Create(ID3D11Device device, SamplerDescription description, string name)
    {
        var state = device.CreateSamplerState(description);
        return new SamplerState(state, name);
    }

    private static SamplerDescription CreateCompareLessEqualClamp()
    {
        return new SamplerDescription()
        {
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            BorderColor = Colors.White,
            ComparisonFunction = ComparisonFunction.LessEqual,
            Filter = Filter.ComparisonAnisotropic, // Should this be ComparisonXYZ or not?
            MaxAnisotropy = 1,
            MaxLOD = float.MaxValue,
            MinLOD = float.MinValue,
            MipLODBias = 0
        };
    }

    private SamplerDescription CreateCompareGreaterClamp()
    {
        return new SamplerDescription()
        {
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            BorderColor = Colors.White,
            ComparisonFunction = ComparisonFunction.Greater,
            Filter = Filter.ComparisonAnisotropic, // Should this be ComparisonXYZ or not?
            MaxAnisotropy = 1,
            MaxLOD = float.MaxValue,
            MinLOD = float.MinValue,
            MipLODBias = 0
        };
    }

    public void Dispose()
    {
        this.PointWrap.Dispose();
        this.PointClamp.Dispose();
        this.LinearWrap.Dispose();
        this.LinearClamp.Dispose();
        this.AnisotropicWrap.Dispose();
        this.CompareLessEqualClamp.Dispose();
        this.CompareGreaterClamp.Dispose();
    }
}
