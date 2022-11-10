using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using ColorComponents = StbImageSharp.ColorComponents;
using ImageResultFloat = StbImageSharp.ImageResultFloat;

namespace Mini.Engine.Content.v2.Textures;
internal sealed class HdrTextureProcessor : UnmanagedContentProcessor<ITexture, TextureContent, TextureLoaderSettings>
{
    private const int ProcessorVersion = 1;
    private static readonly Guid ProcessorType = new("{650531A2-0DF5-4E36-85AB-F5525464F962}");
    
    private readonly Device Device;

    public HdrTextureProcessor(Device device)
        : base(ProcessorVersion, ProcessorType, ".hdr")
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
        var image = ImageResultFloat.FromMemory(bytes, ColorComponents.RedGreenBlue);

        HdrTextureWriter.Write(writer, settings, image);
    }

    protected override TextureLoaderSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadTextureSettings();
    }

    protected override ITexture ReadBody(ContentId id, TextureLoaderSettings settings, ContentReader reader)
    {
        return HdrTextureReader.Read(this.Device, id, reader, settings);
    }   

    public override TextureContent Wrap(ContentId id, ITexture content, TextureLoaderSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }    
}
