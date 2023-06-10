using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class DebugPanel : IEditorPanel
{
    private readonly Device Device;
    private readonly FrameService FrameService;

    private readonly RenderDoc? RenderDoc;

    private uint nextCaptureToOpen;

    public DebugPanel(Device device, FrameService frameService, Services services)
    {
        this.Device = device;
        this.FrameService = frameService;
    
        if (services.TryResolve<RenderDoc>(out var instance))
        {
            this.RenderDoc = instance;
            this.nextCaptureToOpen = uint.MaxValue;
        }
    }

    public string Title => "Debug";

    public void Update(float elapsed)
    {                
        this.ShowVSync();
        this.ShowAA();
        this.ShowRenderDoc();
    } 

    private void ShowRenderDoc()
    {
        if (this.RenderDoc == null)
        {
            ImGui.TextUnformatted("RenderDoc has been disabled");
        }
        else
        {           
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
