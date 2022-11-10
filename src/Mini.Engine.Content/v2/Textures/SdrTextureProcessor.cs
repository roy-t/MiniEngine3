using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures;

internal sealed class SdrTextureProcessor : UnmanagedContentProcessor<ITexture, TextureContent, TextureLoaderSettings>
{
    private const int ProcessorVersion = 6;
    private static readonly Guid ProcessorType = new("{7AED564E-32B4-4F20-B14A-2D209F0BABBD}");
    
    private readonly Device Device;

    public SdrTextureProcessor(Device device)
        : base(ProcessorVersion, ProcessorType, ".jpg", ".jpeg", ".png", ".bmp", ".tga", ".psd", ".gif")
    {
        this.Device = device;    
    }    

    protected override void WriteSettings(ContentId id, TextureLoaderSettings settings, ContentWriter writer)
    {
        writer.Write(settings);
    }

    protected override void WriteBody(ContentId id, TextureLoaderSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var bytes = fileSystem.ReadAllBytes(id.Path);
        var image = Image.FromMemory(bytes);

        SdrTextureWriter.Write(writer, image, settings);
    }

    protected override TextureLoaderSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadTextureSettings();
    }

    protected override ITexture ReadBody(ContentId id, TextureLoaderSettings settings, ContentReader reader)
    {
        return SdrTextureReader.Read(this.Device, id, reader, settings, TranscodeFormats.BC7_RGBA);
    }    

    public override TextureContent Wrap(ContentId id, ITexture content, TextureLoaderSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }   
}
