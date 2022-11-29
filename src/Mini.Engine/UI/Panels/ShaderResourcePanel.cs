using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;
using Mini.Engine.UI.Components;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class ShaderResourcePanel : IPanel
{
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly TextureSelector Selector;

    public ShaderResourcePanel(FrameService frameService, DebugFrameService debugFrameService, UITextureRegistry registry)
    {
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;
        this.Selector = new TextureSelector(registry);
    }

    public string Title => "Shader Resources";

    public void Update(float elapsed)
    {
        if (this.Selector.Begin("Shader Resources", nameof(this.FrameService.GBuffer.Albedo)))
        {
            this.Selector.Select(nameof(this.FrameService.GBuffer.Albedo), this.FrameService.GBuffer.Albedo);
            this.Selector.Select(nameof(this.FrameService.GBuffer.Material), this.FrameService.GBuffer.Material);
            this.Selector.Select(nameof(this.FrameService.GBuffer.Normal), this.FrameService.GBuffer.Normal);
            this.Selector.Select(nameof(this.FrameService.LBuffer.Light), this.FrameService.LBuffer.Light);
            this.Selector.Select(nameof(this.FrameService.PBuffer.PreviousColor), this.FrameService.PBuffer.PreviousColor);
            this.Selector.Select(nameof(this.FrameService.PBuffer.CurrentColor), this.FrameService.PBuffer.CurrentColor);            
            this.Selector.Select(nameof(this.FrameService.GBuffer.DepthStencilBuffer), this.FrameService.GBuffer.DepthStencilBuffer);
            this.Selector.Select(nameof(this.DebugFrameService.DebugOverlay), this.DebugFrameService.DebugOverlay);

            this.Selector.End();
        }

        this.Selector.ShowSelected
        (
            this.FrameService.GBuffer.Albedo,
            this.FrameService.GBuffer.Material,
            this.FrameService.GBuffer.Normal,
            this.FrameService.LBuffer.Light,
            this.FrameService.PBuffer.PreviousColor,
            this.FrameService.PBuffer.CurrentColor,
            this.FrameService.GBuffer.DepthStencilBuffer,
            this.DebugFrameService.DebugOverlay
        );
    }
}
