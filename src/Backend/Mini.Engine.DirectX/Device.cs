using System.Runtime.CompilerServices;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;

[assembly: InternalsVisibleTo("Mini.Engine.Debugging")]
[assembly: InternalsVisibleTo("Mini.Engine.Content")]

namespace Mini.Engine.DirectX;

public sealed class Device : IDisposable
{
    private const Format BackBufferFormat = Format.R8G8B8A8_UNorm;
    private const Format RenderTargetViewFormat = Format.R8G8B8A8_UNorm_SRgb;

    private readonly IntPtr WindowHandle;

    private IDXGISwapChain swapChain = null!;

#if DEBUG
    private static readonly DeviceCreationFlags Flags = DeviceCreationFlags.Debug;
#else
        private static readonly DeviceCreationFlags Flags = DeviceCreationFlags.None;
#endif

    public Device(IntPtr windowHandle, int width, int height)
    {
        this.WindowHandle = windowHandle;
        this.Width = width;
        this.Height = height;

#nullable disable
        _ = D3D11CreateDevice(null, DriverType.Hardware, Flags, null, out var device, out var context);
#nullable restore
        this.ID3D11Device = device;
#if DEBUG
        this.ID3D11Debug = device.QueryInterface<ID3D11Debug>();
#endif
        this.ID3D11DeviceContext = context;

        this.CreateSwapChain(width, height);

        this.SamplerStates = new SamplerStates(device);
        this.BlendStates = new BlendStates(device);
        this.DepthStencilStates = new DepthStencilStates(device);
        this.RasterizerStates = new RasterizerStates(device);

        this.Resources = new LifetimeManager();

        this.ImmediateContext = new ImmediateDeviceContext(this, context, nameof(Device));
    }

    public ImmediateDeviceContext ImmediateContext { get; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool VSync { get; set; } = true;

    public SamplerStates SamplerStates { get; }
    public BlendStates BlendStates { get; }
    public DepthStencilStates DepthStencilStates { get; }
    public RasterizerStates RasterizerStates { get; }

    public LifetimeManager Resources { get; }

    internal ID3D11Device ID3D11Device { get; }
#if DEBUG
    internal ID3D11Debug ID3D11Debug { get; }
#endif
    internal ID3D11DeviceContext ID3D11DeviceContext { get; }

    internal ID3D11Texture2D BackBuffer { get; private set; } = null!;
    internal ID3D11RenderTargetView BackBufferView { get; private set; } = null!;

    public DeferredDeviceContext CreateDeferredContextFor<T>()
    {
        return new(this, this.ID3D11Device.CreateDeferredContext(), typeof(T).Name);
    }

    public void Present()
    {
        if (this.VSync)
        {
            this.swapChain.Present(1, PresentFlags.None);
        }
        else
        {
            this.swapChain.Present(0, PresentFlags.AllowTearing);
        }
    }

    public void Resize(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        this.BackBufferView.Dispose();
        this.BackBuffer.Dispose();

        var swapChainDescription = this.CreateSwapChainDescription(width, height);
        this.swapChain.ResizeBuffers(swapChainDescription.BufferCount,
            swapChainDescription.Width,
            swapChainDescription.Height,
            swapChainDescription.Format,
            swapChainDescription.Flags);

        this.CreateBackBuffer();
    }

    private void CreateBackBuffer()
    {
        this.BackBuffer = this.swapChain.GetBuffer<ID3D11Texture2D1>(0);

        // Explicitly set the RTV to a format with SRGB while the actual backbuffer is a format without SRGB to properly
        // let the output window be gamma corrected. See: https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/converting-data-color-space
        var view = new RenderTargetViewDescription(this.BackBuffer, RenderTargetViewDimension.Texture2D, RenderTargetViewFormat);
        this.BackBufferView = this.ID3D11Device.CreateRenderTargetView(this.BackBuffer, view);
        this.BackBufferView.DebugName = DebugNameGenerator.GetName(nameof(Device), "BackBufferView");
    }

    private IDXGIFactory5 GetDxgiFactory()
    {
        var dxgiFactory = this.ID3D11Device.QueryInterface<IDXGIDevice>()
            ?.GetParent<IDXGIAdapter>()
            ?.GetParent<IDXGIFactory5>() // Requires DXGI 1.5 which was added in the Windows 10 Anniverary edition
            ?? throw new Exception("Could not query for IDXGIAdapter or IDXGIFactory5");
        return dxgiFactory;
    }

    private void CreateSwapChain(int width, int height)
    {
        var dxgiFactory = this.GetDxgiFactory();
        var swapchainDesc = this.CreateSwapChainDescription(width, height);

        this.swapChain = dxgiFactory.CreateSwapChainForHwnd(this.ID3D11Device, this.WindowHandle, swapchainDesc);
        this.CreateBackBuffer();
    }

    private SwapChainDescription1 CreateSwapChainDescription(int width, int height)
    {
        var dxgiFactory = this.GetDxgiFactory();

        return new SwapChainDescription1()
        {
            BufferCount = 2,
            Format = BackBufferFormat,
            AlphaMode = AlphaMode.Unspecified,
            Height = height,
            Width = width,
            Scaling = Scaling.None,
            Stereo = false,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.FlipDiscard,
            BufferUsage = Usage.RenderTargetOutput,
            Flags = dxgiFactory.PresentAllowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None
        };
    }

    public void Dispose()
    {
        this.ID3D11DeviceContext.ClearState();
        this.ID3D11DeviceContext.Flush();

        this.BackBufferView?.Dispose();
        this.ImmediateContext?.Dispose();
        this.SamplerStates?.Dispose();
        this.BlendStates?.Dispose();
        this.DepthStencilStates?.Dispose();
        this.RasterizerStates?.Dispose();

        this.BackBuffer?.Dispose();
        this.swapChain?.Dispose();

        this.ID3D11DeviceContext.Dispose();
#if DEBUG
        this.ID3D11Debug.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail);
#endif
        this.ID3D11Device.Dispose();
    }
}
