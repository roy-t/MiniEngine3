using ImGuiNET;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine.UI.Panels;
internal class AAPanel : IEditorPanel
{
    private readonly FrameService FrameService;

    public AAPanel(FrameService frameService)
    {
        this.FrameService = frameService;
    }

    public string Title => "Anti Aliasing";

    public void Update()
    {
        this.ShowAA();
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
