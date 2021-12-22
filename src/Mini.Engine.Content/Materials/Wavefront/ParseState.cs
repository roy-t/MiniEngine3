using System.Collections.Generic;
using Mini.Engine.Content.Parsers;

namespace Mini.Engine.Content.Materials.Wavefront;

internal sealed record MaterialRecords(string Key, string Albedo, string Metalicness, string Normal, string Roughness, string AmbientOcclusion);

internal class ParseState : IParseState
{
    public List<MaterialRecords> Materials { get; }

    public string? CurrentKey { get; internal set; }

    public string? Albedo { get; internal set; }
    public string? Metalicness { get; internal set; }
    public string? Normal { get; internal set; }
    public string? Roughness { get; internal set; }
    public string? AmbientOcclusion { get; internal set; }

    public ParseState()
    {
        this.Materials = new List<MaterialRecords>();
    }

    public void NewMaterial(string material)
    {
        this.EndMaterial();

        this.CurrentKey = material;
        this.Albedo = null;
    }

    public void EndMaterial()
    {
        if (this.CurrentKey != null)
        {
            var material = new MaterialRecords(this.CurrentKey,
                this.Albedo ?? string.Empty,
                this.Metalicness ?? string.Empty,
                this.Normal ?? string.Empty,
                this.Roughness ?? string.Empty,
                this.AmbientOcclusion ?? string.Empty);

            this.Materials.Add(material);
        }
    }
}
