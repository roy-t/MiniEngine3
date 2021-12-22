using System;
using System.IO;
using Mini.Engine.Content.Models.Wavefront;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models;

internal sealed class ModelLoader : IContentLoader<Model>
{
    private readonly WavefrontModelDataLoader WaveFrontDataLoader;

    public ModelLoader(IVirtualFileSystem fileSystem)
    {
        this.WaveFrontDataLoader = new WavefrontModelDataLoader(fileSystem);
    }

    public Model Load(Device device, ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<ModelData> loader = extension switch
        {
            ".obj" => this.WaveFrontDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported model file type {extension}")
        };

        var data = loader.Load(id);
        return new ModelContent(id, device, loader, data);
    }

    public void Unload(Model content)
    {
        content.Dispose();
    }
}
