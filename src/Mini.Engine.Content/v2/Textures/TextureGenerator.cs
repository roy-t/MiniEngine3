using Mini.Engine.Configuration;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Textures.Readers;
using Mini.Engine.Content.v2.Textures.Writers;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using SuperCompressed;
using Stb = StbImageSharp;

namespace Mini.Engine.Content.v2.Textures;

[Service]
public class TextureGenerator : IContentGenerator<TextureContent>
{    
    private readonly Device Device;

    public TextureGenerator(Device device)
    {
        this.Device = device;
    }

    public string GeneratorKey => nameof(TextureGenerator);

    public void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, ContentWriter contentWriter)
    {
        if (HasSupportedSdrExtension(id))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Image.FromMemory(bytes);
            CompressedTextureWriter.Write(contentWriter, meta, fileSystem.GetDependencies(), image);
        }
        else if (HasSupportedHdrExtension(id))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Stb.ImageResultFloat.FromMemory(bytes, Stb.ColorComponents.RedGreenBlue);
            HdrTextureWriter.Write(contentWriter, meta, fileSystem.GetDependencies(), image);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public TextureContent Load(ContentId id, ContentReader reader)
    {
        var header = reader.ReadHeader();
        var settings = header.Meta.TextureSettings;
        ITexture texture;
        if (header.Type == TextureConstants.HeaderCompressed)
        {
            texture = CompressedTextureReader.Read(this.Device, id, settings, TranscodeFormats.BC7_RGBA, reader);
        }
        else if (header.Type == TextureConstants.HeaderUncompressed)
        {
            texture = CompressedTextureReader.Read(this.Device, id, settings, TranscodeFormats.RGBA32, reader);
        }
        else if (header.Type == TextureConstants.HeaderHdr)
        {

            texture = HdrTextureReader.Read(this.Device, id, settings, reader);
        }
        else
        {
            throw new NotSupportedException($"Unexpected header: {header}");
        }

        return new TextureContent(id, texture, header.Meta, header.Dependencies);
    }

    public void Reload(IContent original, TrackingVirtualFileSystem fileSystem, Stream rwStream)
    {
        TextureReloader.Reload(this, (TextureContent)original, fileSystem, rwStream);        
    }

    public IContentCache CreateCache(IVirtualFileSystem fileSystem)
    {
        return new ContentCache<TextureContent>(this, fileSystem);
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

