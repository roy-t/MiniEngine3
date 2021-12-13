using System.Collections.Generic;
using Mini.Engine.Content.Parsers;

namespace Mini.Engine.Content.Materials.Wavefront;

internal class ParseState : IParseState
{
    private int NextIndex = 0;

    public List<MaterialData> Materials { get; }

    public string? CurrentMaterial { get; internal set; }

    public string? Albedo { get; internal set; }
    public string? Metalicness { get; internal set; }
    public string? Normal { get; internal set; }
    public string? Roughness { get; internal set; }
    public string? AmbientOcclusion { get; internal set; }

    public ParseState()
    {
        this.Materials = new List<MaterialData>();
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
            this.Materials.Add(new MaterialData(this.CurrentMaterial,
                this.NextIndex++,
                this.Albedo ?? string.Empty,
                this.Metalicness ?? string.Empty,
                this.Normal ?? string.Empty,
                this.Roughness ?? string.Empty,
                this.AmbientOcclusion ?? string.Empty));
        }
    }
}
