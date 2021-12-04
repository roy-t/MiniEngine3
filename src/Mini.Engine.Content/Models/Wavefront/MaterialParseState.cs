using System.Collections.Generic;

namespace Mini.Engine.Content.Models.Wavefront;

internal record class Material(string Name, string? Albedo, string? Metalicness, string? Normal, string? Roughness, string? AmbientOcclusion);

internal class MaterialParseState : IParseState
{
    public List<Material> Materials { get; }

    public string? CurrentMaterial { get; internal set; }

    public string? Albedo { get; internal set; }
    public string? Metalicness { get; internal set; }
    public string? Normal { get; internal set; }
    public string? Roughness { get; internal set; }
    public string? AmbientOcclusion { get; internal set; }

    public MaterialParseState()
    {
        this.Materials = new List<Material>();
    }

    public void NewMaterial(string material)
    {
        this.EndMaterial();

        this.CurrentMaterial = material;
        this.Albedo = null;
    }

    public void EndMaterial()
    {
        if (this.CurrentMaterial != null)
        {
            this.Materials.Add(new Material(this.CurrentMaterial,
                this.Albedo,
                this.Metalicness,
                this.Normal,
                this.Roughness,
                this.AmbientOcclusion));
        }
    }
}
