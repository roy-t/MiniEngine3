using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Content.Materials;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.v2.Materials;
public sealed class MaterialContent : IMaterial, IContent<IMaterial, MaterialLoaderSettings>
{
    private IMaterial original;

    public MaterialContent(ContentId id, IMaterial original, MaterialLoaderSettings settings, ISet<string> dependencies)
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
    public MaterialLoaderSettings Settings { get; }
    public ISet<string> Dependencies { get; }

    public string Name => this.original.Name;
    public ILifetime<ISurface> Albedo => this.original.Albedo;
    public ILifetime<ISurface> Metalicness => this.original.Metalicness;
    public ILifetime<ISurface> Normal => this.original.Normal;
    public ILifetime<ISurface> Roughness => this.original.Roughness;
    public ILifetime<ISurface> AmbientOcclusion => this.original.AmbientOcclusion;

    public void Dispose()
    {
        // Do nothing
    }
}
