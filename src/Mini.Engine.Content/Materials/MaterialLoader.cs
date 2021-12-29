using System;
using System.IO;
using Mini.Engine.Content.Materials.Wavefront;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials;

internal sealed class MaterialLoader : IContentLoader<MaterialContent>
{
    private readonly IContentDataLoader<MaterialData> WavefrontMaterialDataLoader;
    private readonly ContentManager Content;

    public MaterialLoader(ContentManager content, IVirtualFileSystem fileSystem, IContentLoader<Texture2DContent> textureLoader)
    {
        this.WavefrontMaterialDataLoader = new WavefrontMaterialDataLoader(fileSystem, textureLoader);
        this.Content = content;
    }

    public MaterialContent Load(Device device, ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<MaterialData> loader = extension switch
        {
            ".mtl" => this.WavefrontMaterialDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported material file extension: {extension}"),
        };

        var content = new MaterialContent(id, device, loader);
        this.Content.Add(content);

        return content;
    }

    // Material is a composition of resources, so it doesn't need to unload anything itself
    public void Unload(MaterialContent content) { }
}
