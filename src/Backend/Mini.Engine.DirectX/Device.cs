using System.Drawing;
using System.Runtime.CompilerServices;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Debugging;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.DXGI.Debug;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

[assembly: InternalsVisibleTo("Mini.Engine.Debugging")]
[assembly: InternalsVisibleTo("Mini.Engine.Content")]

namespace Mini.Engine.DirectX;

public sealed class Device : IDisposable
{
    private const Format BackBufferFormat = Format.R8G8B8A8_UNorm;
    private const Format RenderTargetViewFormat = Format.R8G8B8A8_UNorm_SRgb;

    private readonly IntPtr WindowHandle;

    private readonly IDXGIFactory4 DXGIFactory;
    private readonly bool PresentAllowTearing;
    private IDXGISwapChain swapChain = null!;


#if DEBUG
    private static readonly DeviceCreationFlags Flags = DeviceCreationFlags.Debug;
    private readonly DebugLayerExceptionConverter DebugLayerExceptionConverter;
    private readonly IDXGIInfoQueue DxgiInfoQueue;
    private readonly IDXGIDebug IDXGIDebug;
#else
        private static readonly DeviceCreationFlags Flags = DeviceCreationFlags.None;
#endif

    public Device(IntPtr windowHandle, int width, int height, LifetimeManager lifetimeManager)
    {
        this.WindowHandle = windowHandle;
        this.Viewport = new Rectangle(0, 0, width, height);

#nullable disable
        _ = D3D11CreateDevice(null, DriverType.Hardware, Flags, null, out var device, out var context);
#nullable restore
        this.ID3D11Device = device;
        this.DXGIFactory = this.CreateDxgiFactory(out this.PresentAllowTearing);
#if DEBUG
        this.DebugLayerExceptionConverter = new DebugLayerExceptionConverter();

        this.IDXGIDebug = DXGIGetDebugInterface1<IDXGIDebug>();
        this.DxgiInfoQueue = DXGIGetDebugInterface1<IDXGIInfoQueue>();
        this.DxgiInfoQueue.PushEmptyStorageFilter(DebugAll);
        this.DxgiInfoQueue.SetBreakOnSeverity(DebugAll, InfoQueueMessageSeverity.Warning, true);
        this.DxgiInfoQueue.SetBreakOnSeverity(DebugAll, InfoQueueMessageSeverity.Error, true);
        this.DxgiInfoQueue.SetBreakOnSeverity(DebugAll, InfoQueueMessageSeverity.Corruption, true);
        this.DebugLayerExceptionConverter.Register(this.DxgiInfoQueue, DebugAll);      
#endif
        this.ID3D11DeviceContext = context;

        this.CreateSwapChain(width, height);

        this.SamplerStates = new SamplerStates(device);
        this.BlendStates = new BlendStates(device);
        this.DepthStencilStates = new DepthStencilStates(device);
        this.RasterizerStates = new RasterizerStates(device);

        this.Resources = lifetimeManager;

        this.ImmediateContext = new ImmediateDeviceContext(this, context, nameof(Device));
    }

    public ImmediateDeviceContext ImmediateContext { get; }

    public Rectangle Viewport { get; private set; }
    public int Width => this.Viewport.Width;
    public int Height => this.Viewport.Height;
    public bool VSync { get; set; } = true;

    public SamplerStates SamplerStates { get; }
    public BlendStates BlendStates { get; }
    public DepthStencilStates DepthStencilStates { get; }
    public RasterizerStates RasterizerStates { get; }

    public LifetimeManager Resources { get; }

    internal ID3D11Device ID3D11Device { get; }

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
        this.Viewport = new Rectangle(0, 0, width, height);

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

    private IDXGIFactory4 CreateDxgiFactory(out bool presentAllowTearing)
    {
        using var device = this.ID3D11Device.QueryInterface<IDXGIDevice>();
        using var adapter = device.GetParent<IDXGIAdapter>();

        var factory4 = adapter.GetParent<IDXGIFactory4>();

        // Requires DXGI 1.5 which was added in the Windows 10 Anniverary edition
        using var factory5 = factory4.QueryInterface<IDXGIFactory5>();
        factory5.DebugName = "FACTORY5_DXGI4";
        presentAllowTearing = factory5?.PresentAllowTearing ?? false;
       
        return factory4;
    }

    private void CreateSwapChain(int width, int height)
    {
        var swapchainDesc = this.CreateSwapChainDescription(width, height);

        this.swapChain = this.DXGIFactory.CreateSwapChainForHwnd(this.ID3D11Device, this.WindowHandle, swapchainDesc);
        this.CreateBackBuffer();
    }

    private SwapChainDescription1 CreateSwapChainDescription(int width, int height)
    {
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
            Flags = this.PresentAllowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None
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
        this.DXGIFactory.Dispose();

        this.ID3D11DeviceContext.Dispose();
        this.ID3D11Device.Dispose();

#if DEBUG
        // Avoid not getting a readout of all left over objects, by breaking on the first finding

        this.DxgiInfoQueue.SetBreakOnSeverity(DebugAll, InfoQueueMessageSeverity.Warning, false);
        this.DxgiInfoQueue.SetBreakOnSeverity(DebugAll, InfoQueueMessageSeverity.Error, false);
        this.DxgiInfoQueue.SetBreakOnSeverity(DebugAll, InfoQueueMessageSeverity.Corruption, false);
        
        this.IDXGIDebug.ReportLiveObjects(DebugAll, ReportLiveObjectFlags.Detail | ReportLiveObjectFlags.IgnoreInternal);      
        this.DebugLayerExceptionConverter.CheckExceptions();
#endif                
    }
}
