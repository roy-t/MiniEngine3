using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureLoader : IContentLoader<Texture2DContent>
{
    private readonly TextureDataLoader TextureDataLoader;
    private readonly HdrTextureDataLoader HdrTextureDataLoader;
    private readonly CompressedTextureLoader CompressedTextureLoader;
    private readonly TextureCompressor TextureCompressor;

    private readonly ContentManager Content;

    public TextureLoader(ContentManager content, TextureCompressor textureCompressor, IVirtualFileSystem fileSystem)
    {
        this.Content = content;

        this.TextureCompressor = textureCompressor;
        this.TextureDataLoader = new TextureDataLoader(fileSystem);
        this.HdrTextureDataLoader = new HdrTextureDataLoader(fileSystem);
        this.CompressedTextureLoader = new CompressedTextureLoader(fileSystem, textureCompressor);        
    }

    public Texture2DContent Load(Device device, ContentId id, ILoaderSettings settings)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        IContentDataLoader<TextureData> loader = extension switch
        {
            ".hdr" => this.HdrTextureDataLoader,
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => this.TextureDataLoader, // unused
            ".uastc" => this.CompressedTextureLoader,
            _ => throw new NotSupportedException($"Could not load {id}. Unsupported image file type: {extension}"),
        };

        this.TextureCompressor.Watch(id, (settings as TextureLoaderSettings) ?? TextureLoaderSettings.Default);
       
        var content = new Texture2DContent(id, device, loader, settings);
        this.Content.Add(content);
        return content;
    }

    public void Unload(Texture2DContent texture)
    {
        texture.Dispose();
    }
}
