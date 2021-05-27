using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.Win32;

namespace VorticeImGui
{
    class AppWindow : Win32Window
    {
        ID3D11Device device;
        ID3D11DeviceContext deviceContext;
        IDXGISwapChain swapChain;
        ID3D11Texture2D backBuffer;
        ID3D11RenderTargetView renderView;

        Format format = Format.R8G8B8A8_UNorm;

        ImGuiRenderer imGuiRenderer;
        ImGuiInputHandler imguiInputHandler;
        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan lastFrameTime;

        IntPtr imGuiContext;

        public AppWindow(string title, int width, int height, ID3D11Device device, ID3D11DeviceContext deviceContext)
            : base(title, width, height)
        {
            this.device = device;
            this.deviceContext = deviceContext;

            imGuiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imGuiContext);

            imGuiRenderer = new ImGuiRenderer(this.device, this.deviceContext);
            imguiInputHandler = new ImGuiInputHandler(this.Handle);

            ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);
        }

        public void Show()
        {
            User32.ShowWindow(this.Handle, ShowWindowCommand.Normal);
        }

        public override bool ProcessMessage(uint msg, UIntPtr wParam, IntPtr lParam)
        {
            ImGui.SetCurrentContext(imGuiContext);
            if (imguiInputHandler.ProcessMessage((WindowMessage)msg, wParam, lParam))
                return true;

            return base.ProcessMessage(msg, wParam, lParam);
        }

        public void UpdateAndDraw()
        {
            UpdateImGui();
            render();
        }

        protected override void Resize()
        {
            if (renderView == null)//first show
            {
                var dxgiFactory = device.QueryInterface<IDXGIDevice>().GetParent<IDXGIAdapter>().GetParent<IDXGIFactory>();

                var swapchainDesc = new SwapChainDescription()
                {
                    BufferCount = 1,
                    BufferDescription = new ModeDescription(this.Width, this.Height, format),
                    IsWindowed = true,
                    OutputWindow = this.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Vortice.DXGI.Usage.RenderTargetOutput
                };

                swapChain = dxgiFactory.CreateSwapChain(device, swapchainDesc);
                dxgiFactory.MakeWindowAssociation(this.Handle, WindowAssociationFlags.IgnoreAll);

                backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
                renderView = device.CreateRenderTargetView(backBuffer);
            }
            else
            {
                renderView.Dispose();
                backBuffer.Dispose();

                swapChain.ResizeBuffers(1, this.Width, this.Height, format, SwapChainFlags.None);

                backBuffer = swapChain.GetBuffer<ID3D11Texture2D1>(0);
                renderView = device.CreateRenderTargetView(backBuffer);
                ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);
            }
        }

        public virtual void UpdateImGui()
        {
            ImGui.SetCurrentContext(imGuiContext);
            var io = ImGui.GetIO();

            var now = stopwatch.Elapsed;
            var delta = now - lastFrameTime;
            lastFrameTime = now;
            io.DeltaTime = (float)delta.TotalSeconds;

            imguiInputHandler.Update();

            ImGui.NewFrame();
        }

        void render()
        {
            ImGui.Render();

            var dc = deviceContext;
            dc.ClearRenderTargetView(renderView, new Color4(0, 0, 0));
            dc.OMSetRenderTargets(renderView);
            dc.RSSetViewport(0, 0, this.Width, this.Height);

            imGuiRenderer.Render(ImGui.GetDrawData());
            DoRender();

            swapChain.Present(0, PresentFlags.None);
        }

        public virtual void DoRender() { }
    }
}
