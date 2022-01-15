using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class RenderDocPanel : IPanel
{
    private RenderDoc? renderDoc;

    public RenderDocPanel(Services services)
    {
        RenderDoc? instance;
        if (services.TryResolve<RenderDoc>(out instance))
        {
            this.renderDoc = instance;
        }
    }

    public string Title => "RenderDoc";

    public void Update(float elapsed)
    {
        if (this.renderDoc == null)
        {
            ImGui.Text("RenderDoc has been disabled");
        }
        else
        {
            if (ImGui.Button("Capture"))
            {
                this.renderDoc.TriggerCapture();
            }

            if (this.renderDoc.GetNumCaptures() > 0 && ImGui.Button("Open Last Capture"))
            {
                var path = this.renderDoc.GetCapture(this.renderDoc.GetNumCaptures() - 1) ?? string.Empty;
                this.renderDoc.LaunchReplayUI(path);
            }
        }
    }
}
