using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Content.Materials;

internal record class MaterialData(ContentId Id, ITexture2D Albedo, ITexture2D Metalicness, ITexture2D Normal, ITexture2D Roughness, ITexture2D AmbientOcclusion)
    : IContentData;

internal sealed class MaterialContent : IMaterial, IContent
{
    private readonly IContentDataLoader<MaterialData> Loader;
    private IMaterial material;

    public MaterialContent(ContentId id, Device device, IContentDataLoader<MaterialData> loader)
    {
        this.Id = id;
        this.Loader = loader;

        this.Reload(device);
    }

    public ContentId Id { get; }

    public ITexture2D Albedo => this.material.Albedo;
    public ITexture2D Metalicness => this.material.Metalicness;
    public ITexture2D Normal => this.material.Normal;
    public ITexture2D Roughness => this.material.Roughness;
    public ITexture2D AmbientOcclusion => this.material.AmbientOcclusion;

    [MemberNotNull(nameof(material))]
    public void Reload(Device device)
    {
        var data = this.Loader.Load(device, this.Id);
        this.material = new Material(data.Albedo, data.Metalicness, data.Normal, data.Roughness, data.AmbientOcclusion, this.Id.ToString());
    }

    public override string ToString()
    {
        return $"Material: {this.Id}";
    }
}