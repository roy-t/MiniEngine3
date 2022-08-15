using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class DebugPanel : IPanel
{
    private readonly Device Device;
    private readonly DebugFrameService FrameService;
    private readonly PerformanceCounters Counters;

    private readonly RenderDoc? RenderDoc;

    private RasterizerState[] RasterizerStates;
    private int selectedRasterizerState;

    private uint nextCaptureToOpen;

    public DebugPanel(Device device, DebugFrameService frameService, PerformanceCounters counters, Services services)
    {
        this.Device = device;
        this.FrameService = frameService;
        this.Counters = counters;
        this.RasterizerStates = new RasterizerState[]
        {
            device.RasterizerStates.CullCounterClockwise,
            device.RasterizerStates.CullClockwise,
            device.RasterizerStates.WireFrame,
        };

        if (services.TryResolve<RenderDoc>(out var instance))
        {
            this.RenderDoc = instance;
            this.nextCaptureToOpen = uint.MaxValue;
        }        
    }

    public string Title => "Debug";

    public void Update(float elapsed)
    {                
        this.ShowDebugOverlaySettings();
        this.ShowRasterizerStateSettings();
        this.ShowVSync();
        this.ShowRenderDoc();
    }

    private void ShowRasterizerStateSettings()
    {
        if (ImGui.BeginCombo("Rasterizer State", this.RasterizerStates[this.selectedRasterizerState].Name))
        {
            for (var i = 0; i < this.RasterizerStates.Length; i++)
            {
                var selected = i == this.selectedRasterizerState;
                var name = this.RasterizerStates[i].Name;
                if (ImGui.Selectable(name, selected))
                {
                    this.selectedRasterizerState = i;
                    this.Device.RasterizerStates.Default = this.RasterizerStates[i];
                }
            }
            ImGui.EndCombo();
        }
    }

    private void ShowDebugOverlaySettings()
    {
        var enableDebugOverlay = this.FrameService.EnableDebugOverlay;
        if (ImGui.Checkbox("Enable Debug Overlay", ref enableDebugOverlay))
        {
            this.FrameService.EnableDebugOverlay = enableDebugOverlay;
        }

        var showBounds = this.FrameService.ShowBounds;
        if (ImGui.Checkbox("Show Bounds", ref showBounds))
        {
            this.FrameService.ShowBounds = showBounds;
        }
    }

    private void ShowRenderDoc()
    {
        if (this.RenderDoc == null)
        {
            ImGui.Text("RenderDoc has been disabled");
        }
        else
        {
            var megabytes = this.Counters.GetGPUMemoryUsageBytes() / (1024 * 1024);
            ImGui.LabelText("GPU Memory Usage", $"{megabytes} MB");

            if (ImGui.Button("Capture"))
            {
                this.nextCaptureToOpen = this.RenderDoc.GetNumCaptures() + 1;
                this.RenderDoc.TriggerCapture();
            }

            if (this.RenderDoc.GetNumCaptures() == this.nextCaptureToOpen)
            {
                var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1) ?? string.Empty;
                this.RenderDoc.LaunchReplayUI(path);
                this.nextCaptureToOpen = uint.MaxValue;

            }
        }
    }

    private void ShowVSync()
    {
        var vsync = this.Device.VSync;
        if (ImGui.Checkbox("Enable VSync", ref vsync))
        {
            this.Device.VSync = vsync;
        }
    }
}
