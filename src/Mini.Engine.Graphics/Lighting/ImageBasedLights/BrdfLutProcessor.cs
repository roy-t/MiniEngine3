using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using StbImageSharp;
using Vortice.DXGI;
using IContent = Mini.Engine.Content.v2.IContent;
using ImageInfo = Mini.Engine.DirectX.Resources.Surfaces.ImageInfo;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;
[Service]
public sealed class BrdfLutProcessor : IContentProcessor<TextureContent, TextureLoaderSettings>
{
    internal static readonly Guid Header = new("{0021262F-65A4-4D2C-AF2F-F6FEB7E62229}");

    private const int Resolution = 512;

    private readonly Device Device;
    private readonly BrdfLutCompute Shader;
    private readonly BrdfLutCompute.User User;

    public BrdfLutProcessor(Device device, BrdfLutCompute shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = this.Shader.CreateUserFor<BrdfLutProcessor>();
        this.Cache = new ContentTypeCache<TextureContent>();
    }

    public int Version => 7;
    public IContentTypeCache<TextureContent> Cache { get; }

    public void Generate(ContentId id, TextureLoaderSettings _, ContentWriter contentWriter, TrackingVirtualFileSystem fileSystem)
    {
        var context = this.Device.ImmediateContext;

        var imageInfo = new ImageInfo(Resolution, Resolution, Format.R32G32_Float);
        using var texture = new RWTexture(this.Device, "BrdfLut", imageInfo, MipMapInfo.None());

        this.User.MapConstants(context, Resolution, Resolution);
        context.CS.SetConstantBuffer(BrdfLutCompute.ConstantsSlot, this.User.ConstantsBuffer);
        context.CS.SetShader(this.Shader.BrdfLutKernel);
        context.CS.SetUnorderedAccessView(BrdfLutCompute.Lut, texture);

        var (x, y, z) = this.Shader.BrdfLutKernel.GetDispatchSize(Resolution, Resolution, 1);
        context.CS.Dispatch(x, y, z);
        context.CS.ClearUnorderedAccessView(BrdfLutCompute.Lut);

        var buffer = new float[imageInfo.Pixels * 2];
        context.CopySurfaceDataToTexture<float>(texture, buffer);

        var image = new ImageResultFloat()
        {
            Width = Resolution,
            Height = Resolution,
            SourceComp = ColorComponents.GreyAlpha,
            Comp = ColorComponents.GreyAlpha,
            Data = buffer
        };

        var dependencies = new HashSet<string>() { BrdfLutCompute.SourceFile };
        var settings = new TextureLoaderSettings(SuperCompressed.Mode.Linear, false);
        HdrTextureWriter.Write(contentWriter, Header, this.Version, settings, dependencies, image);
    }

    public TextureContent Load(ContentId id, ContentHeader header, ContentReader contentReader)
    {
        if (header.Type != Header)
        {
            throw new NotSupportedException($"Unexpected header: {header}");
        }

        var (settings, texture) = HdrTextureReader.Read(this.Device, id, contentReader);
        return new TextureContent(id, texture, settings, header.Dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        TextureReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
    }
}
