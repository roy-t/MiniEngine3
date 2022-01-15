using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Graphics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class TexturePanel : IPanel
{
    private string[] RenderTargetNames;
    private RenderTarget2D[] RenderTargets;
    private IntPtr[] RenderTargetIds;
    private int selectedRenderTarget;

    public TexturePanel(FrameService frameService, UITextureRegistry textureRegistry)
    {
        this.RenderTargetNames = new string[]
        {
            nameof(frameService.GBuffer.Albedo),
            nameof(frameService.GBuffer.Material),
            nameof(frameService.GBuffer.Normal),
            nameof(frameService.GBuffer.Depth),
            nameof(frameService.LBuffer.Light)
        };

        this.RenderTargets = new RenderTarget2D[]
        {
            frameService.GBuffer.Albedo,
            frameService.GBuffer.Material,
            frameService.GBuffer.Normal,
            frameService.GBuffer.Depth,
            frameService.LBuffer.Light,
        };

        this.RenderTargetIds = this.RenderTargets.Select(rt => textureRegistry.Register(rt)).ToArray();
    }

    public string Title => "Textures";

    public void Update(float elapsed)
    {
        if (ImGui.BeginCombo("Render Targets", this.RenderTargetNames[this.selectedRenderTarget]))
        {
            for (var i = 0; i < this.RenderTargetNames.Length; i++)
            {
                var isSelected = i == this.selectedRenderTarget;
                if (ImGui.Selectable(this.RenderTargetNames[i], isSelected))
                {
                    this.selectedRenderTarget = i;
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        var selected = this.RenderTargets[this.selectedRenderTarget];
        ImGui.Image(this.RenderTargetIds[this.selectedRenderTarget], Fit(selected, ImGui.GetWindowContentRegionWidth()));
    }


    private static Vector2 Fit(Texture2D texture, float maxWidth)
    {
        var factor = Math.Min(1, maxWidth / texture.Dimensions.X);
        return texture.Dimensions * factor;
    }
}
