using System;
using System.IO;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models.Wavefront;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models;

internal sealed class ModelLoader : IContentLoader<ModelContent>
{
    private readonly WavefrontModelDataLoader WaveFrontDataLoader;
    private readonly IVirtualFileSystem FileSystem;

    public ModelLoader(IVirtualFileSystem fileSystem, IContentLoader<MaterialContent> materialLoader)
    {
        this.WaveFrontDataLoader = new WavefrontModelDataLoader(fileSystem, materialLoader);
        this.FileSystem = fileSystem;
        this.MaterialLoader = materialLoader;
    }

    public IContentLoader<MaterialContent> MaterialLoader { get; }

    public ModelContent Load(Device device, ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<ModelData> loader = extension switch
        {
            ".obj" => this.WaveFrontDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported model file type {extension}")
        };

        var data = loader.Load(device, id);

        this.FileSystem.WatchFile(id.Path);

        return new ModelContent(id, device, loader, data);
    }

    public void Unload(ModelContent content)
    {
        for (var i = 0; i < content.Materials.Length; i++)
        {
            var material = (MaterialContent)content.Materials[i];
            this.MaterialLoader.Unload(material);
        }

        content.Dispose();
    }
}
