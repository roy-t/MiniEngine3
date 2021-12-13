using System;
using System.IO;
using Mini.Engine.Content.Models.Wavefront;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models;

internal sealed class ModelLoader : IContentLoader<Model>
{
    private readonly WavefrontModelDataLoader WaveFrontDataLoader;
    private readonly IContentLoader<Texture2DContent> TextureLoader;

    public ModelLoader(IVirtualFileSystem fileSystem, IContentLoader<Texture2DContent> textureLoader)
    {
        this.WaveFrontDataLoader = new WavefrontModelDataLoader(fileSystem);
        this.TextureLoader = textureLoader;
    }

    public Model Load(Device device, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        IContentDataLoader<ModelData> loader = extension switch
        {
            ".obj" => this.WaveFrontDataLoader,
            _ => throw new NotSupportedException($"Could not load {fileName}. Unsupported model file type {extension}")
        };

        var data = loader.Load(fileName);
        return new ModelContent(device, loader, this.TextureLoader, data, fileName);
    }

    public void Unload(Model content)
    {
        content.Dispose();
    }
}
