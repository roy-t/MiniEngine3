using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.Content.v2.Textures.Readers;
using Mini.Engine.Content.v2.Textures.Writers;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using StbImageSharp;
using Vortice.DXGI;
using IContent = Mini.Engine.Content.v2.IContent;
using ImageInfo = Mini.Engine.DirectX.Resources.Surfaces.ImageInfo;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;
[Service]
public sealed class BrdfLutGenerator : IContentTypeManager<TextureContent, TextureLoaderSettings>
{
    private const int Resolution = 512;

    private readonly Device Device;
    private readonly BrdfLutCompute Shader;
    private readonly BrdfLutCompute.User User;

    public BrdfLutGenerator(Device device, BrdfLutCompute shader)
    {
        this.Device = device;
        this.Shader = shader;
        this.User = this.Shader.CreateUserFor<BrdfLutGenerator>();
        this.Cache = new ContentTypeCache<TextureContent>();
    }

    public int Version => 6;
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
        var meta = new ContentRecord(new TextureLoaderSettings(SuperCompressed.Mode.Linear, false));

        var settings = new TextureLoaderSettings(SuperCompressed.Mode.Linear, false);
        HdrTextureWriter.Write(contentWriter, this.Version, settings, dependencies, image);
    }

    public TextureContent Load(ContentId id, ContentHeader header, ContentReader contentReader)
    {
        var (settings, texture) = HdrTextureReader.Read(this.Device, id, contentReader);
        return new TextureContent(id, texture, settings, header.Dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        TextureReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
    }
}
