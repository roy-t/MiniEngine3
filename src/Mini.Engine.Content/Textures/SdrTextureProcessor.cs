using Mini.Engine.Content.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;

internal sealed class SdrTextureProcessor : ContentProcessor<ITexture, TextureContent, TextureSettings>
{
    private const int ProcessorVersion = 6;
    private static readonly Guid ProcessorType = new("{7AED564E-32B4-4F20-B14A-2D209F0BABBD}");

    private readonly Device Device;

    public SdrTextureProcessor(Device device)
        : base(device.Resources, ProcessorVersion, ProcessorType, ".jpg", ".jpeg", ".png", ".bmp", ".tga", ".psd", ".gif")
    {
        this.Device = device;
    }

    protected override void WriteSettings(ContentId id, TextureSettings settings, ContentWriter writer)
    {
        writer.Write(settings);
    }

    protected override void WriteBody(ContentId id, TextureSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var bytes = fileSystem.ReadAllBytes(id.Path);
        var image = Image.FromMemory(bytes);

        SdrTextureWriter.Write(writer, image, settings);
    }

    protected override TextureSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadTextureSettings();
    }

    protected override ITexture ReadBody(ContentId id, TextureSettings settings, ContentReader reader)
    {
        return SdrTextureReader.Read(this.Device, id, reader, settings, TranscodeFormats.BC7_RGBA);
    }

    public override TextureContent Wrap(ContentId id, ITexture content, TextureSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }
}
