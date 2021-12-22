using System;
using System.IO;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureLoader : IContentLoader<Texture2DContent>
{
    private readonly TextureDataLoader TextureDataLoader;
    private readonly HdrTextureDataLoader HdrTextureDataLoader;

    public TextureLoader(IVirtualFileSystem fileSystem)
    {
        this.TextureDataLoader = new TextureDataLoader(fileSystem);
        this.HdrTextureDataLoader = new HdrTextureDataLoader(fileSystem);
    }

    public Texture2DContent Load(Device device, ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<TextureData> loader = extension switch
        {
            ".hdr" => this.HdrTextureDataLoader,
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => this.TextureDataLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported image file type: {extension}"),
        };
        var data = loader.Load(device, id);
        return new Texture2DContent(id, device, loader, data);
    }

    public void Unload(Texture2DContent texture)
    {
        texture.Dispose();
    }
}
