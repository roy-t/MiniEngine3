using Mini.Engine.DirectX;

namespace Mini.Engine.Content.Models;

internal sealed class ModelContent : Model, IContent
{
    private readonly IContentDataLoader<ModelData> DataLoader;
    private readonly IContentLoader<Material> MaterialLoader;

    public ModelContent(Device device, IContentDataLoader<ModelData> loader, IContentLoader<Material> materialLoader, ModelData data, string fileName)
        : base(device)
    {
        this.DataLoader = loader;
        this.MaterialLoader = materialLoader;
        this.Id = fileName;

        this.SetData(device, data);
    }

    public string Id { get; }

    public void Reload(Device device)
    {
        for (var i = 0; i < this.Materials.Length; i++)
        {
            this.MaterialLoader.Unload(this.Materials[i]);
        }
        this.SetData(device, this.DataLoader.Load(this.Id));
    }

    private void SetData(Device device, ModelData data)
    {
        this.Primitives = data.Primitives;
        this.Materials = new Material[data.Materials.Length];

        for (var i = 0; i < this.Materials.Length; i++)
        {
            this.Materials[i] = this.MaterialLoader.Load(device, data.Materials[i].Id);
        }

        this.Vertices.MapData(device.ImmediateContext, data.Vertices);
        this.Indices.MapData(device.ImmediateContext, data.Indices);
    }

    private void UnloadMaterials()
    {

    }
}
