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

        var content = new MaterialContent(id, device, loader);
        this.Content.Add(content);

        return content;
    }

    public void Unload(MaterialContent content)
    {
        this.Unload(content.Albedo);
        this.Unload(content.Metalicness);
        this.Unload(content.Normal);
        this.Unload(content.Roughness);
        this.Unload(content.AmbientOcclusion);
    }

    private void Unload(ITexture2D texture)
    {
        if (texture is Texture2DContent content)
        {
            this.TextureLoader.Unload(content);
        }
        else
        {
            texture.Dispose();
        }
    }
}
