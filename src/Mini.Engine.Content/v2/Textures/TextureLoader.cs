using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures;
internal class TextureLoader
{
    private static readonly Guid Header = new("{650531A2-0DF5-4E36-85AB-2AC5A01DBEA2}");

    //private readonly ResourceManager Resources;

    // TODO: create a separate one for HDRTextures

    public void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, ContentWriter writer)
    {
        if (!HasSupportedExtension(id))
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }

        var bytes = fileSystem.ReadAllBytes(id.Path);
        var image = Image.FromMemory(bytes);
        var encoded = Encoder.Instance.Encode(image, meta.TextureSettings.Mode, MipMapGeneration.Lanczos3, Quality.Default);        
        fileSystem.GetDependencies();

        // TODO: WIP
        writer.WriteAll(Header, meta, fileSystem.GetDependencies(), encoded);
    }

    
    public override IResource<ITexture> Upload(Device device, ContentId id, Stream stream)
    {

    }


    private static bool HasSupportedExtension(ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => true,
            _ => false
        };
    }
}

