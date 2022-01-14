using System;
using System.IO;
using Mini.Engine.Content.Materials.Wavefront;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Vortice.DXGI;

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

    public MaterialContent Load(Device device, ContentId id, ILoaderSettings settings)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<MaterialData> loader = extension switch
        {
            ".mtl" => this.WavefrontMaterialDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported material file extension: {extension}"),
        };

        var content = new MaterialContent(id, device, loader, settings);
        this.Content.Add(content);

        return content;
    }

    // The texture loader makes sure to dispose of any textures, there are no other
    // disposable resource types in a material that we need to take care of manually
    public void Unload(MaterialContent content) { }
}
