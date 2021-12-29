using System;
using System.IO;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models.Wavefront;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models;

internal sealed class ModelLoader : IContentLoader<ModelContent>
{
    private readonly IContentDataLoader<ModelData> WaveFrontDataLoader;
    private readonly ContentManager Content;
    public ModelLoader(ContentManager content, IVirtualFileSystem fileSystem, IContentLoader<MaterialContent> materialLoader)
    {
        this.WaveFrontDataLoader = new WavefrontModelDataLoader(fileSystem, materialLoader);
        this.Content = content;
    }

    public ModelContent Load(Device device, ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<ModelData> loader = extension switch
        {
            ".obj" => this.WaveFrontDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported model file type {extension}")
        };


        var content = new ModelContent(id, device, loader);
        this.Content.Add(content);

        return content;
    }

    public void Unload(ModelContent content)
    {
        content.Dispose();
    }
}
