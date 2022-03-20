using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Graphics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class ShaderResourcePanel : IPanel
{
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly UITextureRegistry TextureRegistry;

    private int selected;
    private string selectedName;
    private ITexture2D? selectedTexture;

    public ShaderResourcePanel(FrameService frameService, DebugFrameService debugFrameService, UITextureRegistry textureRegistry)
    {
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;
        this.TextureRegistry = textureRegistry;

        this.selected = 0;
        this.selectedName = nameof(this.FrameService.GBuffer.Albedo);
        this.selectedTexture = this.FrameService.GBuffer.Albedo;
    }

    public string Title => "Shader Resources";

    public void Update(float elapsed)
    {
        if (ImGui.BeginCombo("Shader Resources", this.selectedName))
        {
            var i = 0;
            this.Selectable(nameof(this.FrameService.GBuffer.Albedo), this.FrameService.GBuffer.Albedo, i++);
            this.Selectable(nameof(this.FrameService.GBuffer.Material), this.FrameService.GBuffer.Material, i++);
            this.Selectable(nameof(this.FrameService.GBuffer.Normal), this.FrameService.GBuffer.Normal, i++);
            this.Selectable(nameof(this.FrameService.LBuffer.Light), this.FrameService.LBuffer.Light, i++);
            this.Selectable(nameof(this.FrameService.GBuffer.DepthStencilBuffer), this.FrameService.GBuffer.DepthStencilBuffer, i++);
            this.Selectable(nameof(this.DebugFrameService.DebugOverlay), this.DebugFrameService.DebugOverlay, i++);

            ImGui.EndCombo();
        }

        ImGui.Image(this.TextureRegistry.Get(this.selectedTexture!), Fit(this.selectedTexture!, ImGui.GetWindowContentRegionMax().X));
    }

    private void Selectable(string name, ITexture2D texture, int index)
    {
        var isSelected = this.selected == index;
        if (ImGui.Selectable(name, isSelected))
        {
            this.selected = index;
            this.selectedName = name;
            this.selectedTexture = texture;            
        }

        if (isSelected)
        {
            ImGui.SetItemDefaultFocus();
        }
    }

    private static Vector2 Fit(ITexture2D texture, float maxWidth)
    {
        var dimensions = new Vector2(texture.Width, texture.Height);
        var factor = Math.Min(1, maxWidth / dimensions.X);
        return dimensions * factor;
    }
}
