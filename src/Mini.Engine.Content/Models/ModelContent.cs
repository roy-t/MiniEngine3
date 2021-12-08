using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content.Models;

internal record class MaterialData(string FileName, int Index, string Albedo, string Metalicness, string Normal, string Roughness, string AmbientOcclusion);

internal sealed record ModelData(string FileName, ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives, MaterialData[] Materials)
: IContentData;

internal sealed class ModelContent : Model, IContent
{
    private readonly IContentDataLoader<ModelData> DataLoader;
    private readonly IContentLoader<Texture2DContent> TextureLoader;

    public ModelContent(Device device, IContentDataLoader<ModelData> loader, IContentLoader<Texture2DContent> textureLoader, ModelData data, string fileName)
        : base(device)
    {
        this.DataLoader = loader;
        this.TextureLoader = textureLoader;
        this.FileName = fileName;

        this.SetData(device, data);
    }

    public string FileName { get; }

    public void Reload(Device device)
    {
        var data = this.DataLoader.Load(this.FileName);
        this.SetData(device, data);
    }

    private void SetData(Device device, ModelData data)
    {
        this.Primitives = data.Primitives;
        this.Materials = new Material[data.Materials.Length];

        for (var i = 0; i < this.Materials.Length; i++)
        {
            var reference = data.Materials[i];

            var albedo = this.TextureLoader.Load(device, reference.Albedo);
            var metalicness = this.TextureLoader.Load(device, reference.Metalicness);
            var normal = this.TextureLoader.Load(device, reference.Normal);
            var roughness = this.TextureLoader.Load(device, reference.Roughness);
            var ambientOcclusion = this.TextureLoader.Load(device, reference.AmbientOcclusion);

            this.Materials[i] = new Material(reference.FileName, albedo, metalicness, normal, roughness, ambientOcclusion);
        }

        this.Vertices.MapData(device.ImmediateContext, data.Vertices);
        this.Indices.MapData(device.ImmediateContext, data.Indices);
    }
}
