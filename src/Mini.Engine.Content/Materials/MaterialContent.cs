using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.Materials;

internal record class MaterialData(ContentId Id, ISurface Albedo, ISurface Metalicness, ISurface Normal, ISurface Roughness, ISurface AmbientOcclusion)
    : IContentData;

internal sealed class MaterialContent : IMaterial, IContent, IDisposable
{
    private readonly IContentDataLoader<MaterialData> Loader;
    private readonly ILoaderSettings Settings;
    private IMaterial material;

    public MaterialContent(ContentId id, Device device, IContentDataLoader<MaterialData> loader, ILoaderSettings settings)
    {
        this.Id = id;
        this.Loader = loader;
        this.Settings = settings;
        this.Reload(device);
    }

    public ContentId Id { get; }

    public string Name => this.material.Name;
    public ILifetime<ISurface> Albedo => this.material.Albedo;
    public ILifetime<ISurface> Metalicness => this.material.Metalicness;
    public ILifetime<ISurface> Normal => this.material.Normal;
    public ILifetime<ISurface> Roughness => this.material.Roughness;
    public ILifetime<ISurface> AmbientOcclusion => this.material.AmbientOcclusion;    

    [MemberNotNull(nameof(material))]
    public void Reload(Device device)
    {
        var data = this.Loader.Load(device, this.Id, this.Settings);        
        this.material = new Material(this.Id.ToString(), device.Resources.Add(data.Albedo), device.Resources.Add(data.Metalicness), device.Resources.Add(data.Normal), device.Resources.Add(data.Roughness), device.Resources.Add(data.AmbientOcclusion));
    }

    public void Dispose()
    {
        // Do not dispose anything as a material is not the only owner of a texture
    }

    public override string ToString()
    {
        return $"Material: {this.Id}";
    }
}