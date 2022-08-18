using System;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.UI.Components;

internal sealed class TextureSelector
{
    private readonly UITextureRegistry TextureRegistry;
    private int index;
    private int selected;
    private string selectedName;

    public TextureSelector(UITextureRegistry textureRegistry)
    {
        this.TextureRegistry = textureRegistry;
        this.selectedName = string.Empty;
        this.selected = -1;
    }
    
    public bool Begin(string name, string fallbackName, int defaultSelection = 0)
    {
        this.index = 0;
        if(this.selected == -1)
        {
            this.selected = defaultSelection;
        }
        
        return ImGui.BeginCombo(name, this.selectedName);
    }

    public void Select(string name, ISurface texture)
    {
        this.Selectable(name, texture, this.index);
        this.index++;
    }

    public void End()
    {
        ImGui.EndCombo();        
    }

    public void ShowSelected(params ISurface[] textures)
    {        
        if (textures.Length > 0)
        {
            var index = this.selected < textures.Length ? this.selected : 0;
            var selectedTexture = textures[index];
            ImGui.Image(this.TextureRegistry.Get(selectedTexture), Fit(selectedTexture, ImGui.GetWindowContentRegionMax().X));
        }
    }

    private void Selectable(string name, ISurface texture, int index)
    {
        var isSelected = this.selected == index;
        if (ImGui.Selectable(name, isSelected))
        {
            this.selected = index;
            this.selectedName = name;
        }

        if (isSelected)
        {            
            ImGui.SetItemDefaultFocus();
        }
    }

    private static Vector2 Fit(ISurface texture, float maxWidth)
    {
        var dimensions = new Vector2(texture.DimX, texture.DimY);
        var factor = Math.Min(1, maxWidth / dimensions.X);
        return dimensions * factor;
    }
}