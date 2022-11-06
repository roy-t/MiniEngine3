using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using StbImageSharp;
using Vortice.DXGI;
using IContent = Mini.Engine.Content.v2.IContent;
using ImageInfo = Mini.Engine.DirectX.Resources.Surfaces.ImageInfo;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class BrdfLutProcessor : IUnmanagedContentProcessor<ITexture, TextureContent, TextureLoaderSettings>
{
    private const int Resolution = 512;

    private static readonly Guid HeaderBrdfLut = new("{0021262F-65A4-4D2C-AF2F-F6FEB7E62229}");

    private readonly Device Device;
    private readonly BrdfLutCompute Shader;
    private readonly BrdfLutCompute.User User;

    public BrdfLutProcessor(Device device, BrdfLutCompute shader)
    {
        this.Device = device;
        this.Cache = new ContentTypeCache<ILifetime<ITexture>>();

        this.Shader = shader;
        this.User = this.Shader.CreateUserFor<BrdfLutProcessor>();
    }

    public int Version => 8;
    public IContentTypeCache<ILifetime<ITexture>> Cache { get; }

    public void Generate(ContentId id, TextureLoaderSettings _, ContentWriter contentWriter, TrackingVirtualFileSystem fileSystem)
    {
        var image = this.ComputeImage();
        var dependencies = new HashSet<string>() { BrdfLutCompute.SourceFile };
        var settings = new TextureLoaderSettings(SuperCompressed.Mode.Linear, false);
        HdrTextureWriter.Write(contentWriter, HeaderBrdfLut, this.Version, settings, dependencies, image);
    }

    public ITexture Load(ContentId id, ContentHeader header, ContentReader contentReader)
    {
        ContentProcessor.ValidateHeader(HeaderBrdfLut, this.Version, header);
        return HdrTextureReader.Read(this.Device, id, contentReader);        
    }

    public TextureContent Wrap(ContentId id, ITexture content, TextureLoaderSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (TextureContent)original, fileSystem, writerReader);
    }

    private ImageResultFloat ComputeImage()
    {
        var context = this.Device.ImmediateContext;

        var imageInfo = new ImageInfo(Resolution, Resolution, Format.R32G32_Float);
        using var texture = new RWTexture(this.Device, "BrdfLut", imageInfo, MipMapInfo.None());
        this.User.MapConstants(context, Resolution, Resolution);
        context.CS.SetConstantBuffer(BrdfLutCompute.ConstantsSlot, this.User.ConstantsBuffer);
        context.CS.SetShader(this.Shader.BrdfLutKernel);
        context.CS.SetUnorderedAccessView(BrdfLutCompute.Lut, texture);

        var (x, y, z) = this.Shader.GetDispatchSizeForBrdfLutKernel(Resolution, Resolution, 1);
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

        return image;
    }
}
