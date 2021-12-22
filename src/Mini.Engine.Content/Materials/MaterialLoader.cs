using System;
using System.IO;
using Mini.Engine.Content.Materials.Wavefront;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials;

internal sealed class MaterialLoader : IContentLoader<MaterialContent>
{
    private readonly IContentDataLoader<MaterialData> WavefrontMaterialDataLoader;

    public MaterialLoader(IVirtualFileSystem fileSystem, IContentLoader<Texture2DContent> textureLoader)
    {
        this.WavefrontMaterialDataLoader = new WavefrontMaterialDataLoader(fileSystem, textureLoader);
        this.TextureLoader = textureLoader;
    }

    public IContentLoader<Texture2DContent> TextureLoader { get; }

    public MaterialContent Load(Device device, ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<MaterialData> loader = extension switch
        {
            ".mtl" => this.WavefrontMaterialDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported material file extension: {extension}"),
        };

        var data = loader.Load(device, id);
        return new MaterialContent(id, loader, data);
    }

    public void Unload(MaterialContent content)
    {
        this.TextureLoader.Unload((Texture2DContent)content.Albedo);
        this.TextureLoader.Unload((Texture2DContent)content.Metalicness);
        this.TextureLoader.Unload((Texture2DContent)content.Normal);
        this.TextureLoader.Unload((Texture2DContent)content.Roughness);
        this.TextureLoader.Unload((Texture2DContent)content.AmbientOcclusion);
    }
}
