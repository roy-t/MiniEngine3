using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content.Materials;

internal record class MaterialData(ContentId Id, Texture2DContent Albedo, Texture2DContent Metalicness, Texture2DContent Normal, Texture2DContent Roughness, Texture2DContent AmbientOcclusion)
    : IContentData;

internal sealed class MaterialContent : Material, IContent
{
    private readonly IContentDataLoader<MaterialData> Loader;

    public MaterialContent(ContentId id, IContentDataLoader<MaterialData> loader, MaterialData data)
        : base(id.ToString(), data.Albedo, data.Metalicness, data.Normal, data.Roughness, data.AmbientOcclusion)
    {
        this.Id = id;
        this.Loader = loader;
    }

    public ContentId Id { get; }

    public void Reload(Device device)
    {
        var data = this.Loader.Load(device, this.Id);

        this.Albedo = data.Albedo;
        this.Metalicness = data.Metalicness;
        this.Normal = data.Normal;
        this.Roughness = data.Roughness;
        this.AmbientOcclusion = data.AmbientOcclusion;
    }
}
