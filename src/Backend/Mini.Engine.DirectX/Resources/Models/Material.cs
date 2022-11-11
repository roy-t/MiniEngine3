using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.DirectX.Resources.Models;

public sealed class Material : IMaterial
{
    public Material(string name, ISurface albedo, ISurface metalicness, ISurface normal, ISurface roughness, ISurface ambientOcclusion)
    {
        this.Name = name;
        this.Albedo = albedo;
        this.Metalicness = metalicness;
        this.Normal = normal;
        this.Roughness = roughness;
        this.AmbientOcclusion = ambientOcclusion;
    }

    public string Name { get; }
    public ISurface Albedo { get; }
    public ISurface Metalicness { get; }
    public ISurface Normal { get; }
    public ISurface Roughness { get; }
    public ISurface AmbientOcclusion { get; }

    public void Dispose()
    {
        // Do nothing, the surfaces are not owned by the material
    }
}
