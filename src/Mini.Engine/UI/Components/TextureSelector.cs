using System;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.UI.Components;

[Service]
internal sealed class TextureSelector
{
    private readonly UITextureRegistry TextureRegistry;
    private int index;
    private int selected;
    private string selectedName;
    private ITexture2D? selectedTexture;

    public TextureSelector(UITextureRegistry textureRegistry)
    {
        this.TextureRegistry = textureRegistry;
        this.selectedName = string.Empty;
    }
    
    public bool Begin(string name)
    {
        this.index = 0;
        return ImGui.BeginCombo(name, this.selectedName);
    }

    public void Select(string name, ITexture2D texture)
    {
        this.Selectable(name, texture, this.index);
        this.index++;
    }

    public void End()
    {
        ImGui.EndCombo();        
    }

    public void ShowSelected()
    {
        if (this.selectedTexture != null)
        {
            ImGui.Image(this.TextureRegistry.Get(this.selectedTexture), Fit(this.selectedTexture, ImGui.GetWindowContentRegionMax().X));
        }
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