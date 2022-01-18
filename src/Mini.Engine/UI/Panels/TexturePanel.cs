﻿using System;
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
    private readonly string[] Names;
    private readonly ITexture2D[] Textures;
    private readonly IntPtr[] Ids;
    private int selected;

    public TexturePanel(FrameService frameService, UITextureRegistry textureRegistry)
    {
        this.Names = new string[]
        {
            nameof(frameService.GBuffer.Albedo),
            nameof(frameService.GBuffer.Material),
            nameof(frameService.GBuffer.Normal),
            nameof(frameService.GBuffer.Depth),
            nameof(frameService.LBuffer.Light),
            nameof(frameService.GBuffer.DepthStencilBuffer)
        };

        this.Textures = new ITexture2D[]
        {
            frameService.GBuffer.Albedo,
            frameService.GBuffer.Material,
            frameService.GBuffer.Normal,
            frameService.GBuffer.Depth,
            frameService.LBuffer.Light,
            frameService.GBuffer.DepthStencilBuffer

        };

        this.Ids = this.Textures.Select(rt => textureRegistry.Register(rt)).ToArray();
    }

    public string Title => "Textures";

    public void Update(float elapsed)
    {
        if (ImGui.BeginCombo("Render Targets", this.Names[this.selected]))
        {
            for (var i = 0; i < this.Names.Length; i++)
            {
                var isSelected = i == this.selected;
                if (ImGui.Selectable(this.Names[i], isSelected))
                {
                    this.selected = i;
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        var selected = this.Textures[this.selected];
        ImGui.Image(this.Ids[this.selected], Fit(selected, ImGui.GetWindowContentRegionWidth()));
    }


    private static Vector2 Fit(ITexture2D texture, float maxWidth)
    {
        var factor = Math.Min(1, maxWidth / texture.Dimensions.X);
        return texture.Dimensions * factor;
    }
}
