using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.Materials;
public sealed class MaterialContent : IMaterial, IContent<IMaterial, MaterialSettings>
{
    private IMaterial original;

    public MaterialContent(ContentId id, IMaterial original, MaterialSettings settings, ISet<string> dependencies)
    {
        this.Id = id;
        this.Settings = settings;
        this.Dependencies = dependencies;

        this.Reload(original);
    }

    [MemberNotNull(nameof(original))]
    public void Reload(IMaterial original)
    {
        this.Dispose();
        this.original = original;
    }

    public ContentId Id { get; }
    public MaterialSettings Settings { get; }
    public ISet<string> Dependencies { get; }

    public string Name => this.original.Name;

    public ISurface Albedo => this.original.Albedo;
    public ISurface Metalicness => this.original.Metalicness;
    public ISurface Normal => this.original.Normal;
    public ISurface Roughness => this.original.Roughness;
    public ISurface AmbientOcclusion => this.original.AmbientOcclusion;

    public void Dispose()
    {
        // Do nothing
    }
}
