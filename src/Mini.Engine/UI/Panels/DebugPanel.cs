﻿using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class DebugPanel : IPanel
{
    private readonly Device Device;
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly PerformanceCounters Counters;

    private readonly RenderDoc? RenderDoc;
    private readonly PostProcessingSystem PostProcessingSystem;

    private RasterizerState[] RasterizerStates;
    private int selectedRasterizerState;

    private uint nextCaptureToOpen;

    public DebugPanel(Device device, FrameService frameService, DebugFrameService debugFrameService, PerformanceCounters counters, Services services)
    {
        this.Device = device;
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;
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

        this.PostProcessingSystem = services.Resolve<PostProcessingSystem>();
    }

    public string Title => "Debug";

    public void Update(float elapsed)
    {                
        this.ShowDebugOverlaySettings();
        this.ShowRasterizerStateSettings();
        this.ShowVSync();
        this.ShowAA();
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
        var enableDebugOverlay = this.DebugFrameService.EnableDebugOverlay;
        if (ImGui.Checkbox("Enable Debug Overlay", ref enableDebugOverlay))
        {
            this.DebugFrameService.EnableDebugOverlay = enableDebugOverlay;
        }

        var showBounds = this.DebugFrameService.ShowBounds;
        if (ImGui.Checkbox("Show Bounds", ref showBounds))
        {
            this.DebugFrameService.ShowBounds = showBounds;
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
            var gpuMB = this.Counters.GPUMemoryCounter.Value / (1024 * 1024);
            ImGui.LabelText("GPU Memory Usage", $"{gpuMB:F2} MB");

            var gpuUsage = this.Counters.GPUUsageCounter.Value;
            ImGui.LabelText("GPU Usage", $"{gpuUsage:F2} %%");

            var cpuMB = this.Counters.CPUMemoryCounter.Value / (1024 * 1024);
            ImGui.LabelText("CPU Memory Usage", $"{cpuMB:F2} MB");

            var cpuUsage = this.Counters.CPUUsageCounter.Value;
            ImGui.LabelText("CPU Usage", $"{cpuUsage:F2} %%");

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

    private static readonly AAType[] AATypes = Enum.GetValues<AAType>();
    private static readonly string[] AANames = Enum.GetValues<AAType>().Select(a => a.ToString()).ToArray();
    private int AAIndex;
    private void ShowAA()
    {
        this.AAIndex = this.FrameService.PBuffer.AntiAliasing switch
        {
            AAType.None => 0,
            AAType.FXAA => 1,
            AAType.TAA => 2,
            _ => throw new Exception("Unsupported AA type in UI"),
        };

        if (ImGui.Combo("Anti Aliasing", ref this.AAIndex, AANames, AANames.Length))
        {
            this.FrameService.PBuffer.AntiAliasing = AATypes[this.AAIndex];            
        }
    }
}
