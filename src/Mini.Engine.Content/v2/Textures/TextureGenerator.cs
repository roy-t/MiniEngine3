using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Textures.Readers;
using Mini.Engine.Content.v2.Textures.Writers;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using SuperCompressed;
using Stb = StbImageSharp;

namespace Mini.Engine.Content.v2.Textures;

internal sealed class TextureGenerator : IContentTypeManager<TextureContent, TextureLoaderSettings>
{
    private readonly Device Device;

    public TextureGenerator(Device device)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<TextureContent>();
    }

    public int Version => 2;
    public IContentTypeCache<TextureContent> Cache { get; }

    public void Generate(ContentId id, TextureLoaderSettings settings, ContentWriter contentWriter, TrackingVirtualFileSystem fileSystem)
    {
        if (HasSupportedSdrExtension(id))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Image.FromMemory(bytes);
            CompressedTextureWriter.Write(contentWriter, this.Version, settings, fileSystem.GetDependencies(), image);
        }
        else if (HasSupportedHdrExtension(id))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Stb.ImageResultFloat.FromMemory(bytes, Stb.ColorComponents.RedGreenBlue);
            HdrTextureWriter.Write(contentWriter, this.Version, settings, fileSystem.GetDependencies(), image);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public TextureContent Load(ContentId id, ContentHeader header, ContentReader reader)
    {
                TextureLoaderSettings settings;
        ITexture texture;
        if (header.Type == TextureConstants.HeaderCompressed)
        {
            (settings, texture) = CompressedTextureReader.Read(this.Device, id, TranscodeFormats.BC7_RGBA, reader);
        }
        else if (header.Type == TextureConstants.HeaderUncompressed)
        {
            (settings, texture) = CompressedTextureReader.Read(this.Device, id, TranscodeFormats.RGBA32, reader);
        }
        else if (header.Type == TextureConstants.HeaderHdr)
        {

            (settings, texture) = HdrTextureReader.Read(this.Device, id, reader);
        }
        else
        {
            throw new NotSupportedException($"Unexpected header: {header}");
        }

        return new TextureContent(id, texture, settings, header.Dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        TextureReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
    }

    private static bool HasSupportedSdrExtension(ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => true,
            _ => false
        };
    }

    private static bool HasSupportedHdrExtension(ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        return extension switch
        {
            ".hdr" => true,
            _ => false
        };
    }
        

}

