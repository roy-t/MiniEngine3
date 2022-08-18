using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.DirectX.Resources;

public sealed class Material : IMaterial
{
    public Material(ISurface albedo, ISurface metalicness, ISurface normal, ISurface roughness, ISurface ambientOcclusion, string user)
    {
        this.Name = DebugNameGenerator.GetName(user);
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
        this.Albedo.Dispose();
        this.Metalicness.Dispose();
        this.Normal.Dispose();
        this.Roughness.Dispose();
        this.AmbientOcclusion.Dispose();
    }
}
