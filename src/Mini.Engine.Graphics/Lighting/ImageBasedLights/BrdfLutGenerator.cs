using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.v2;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.DXGI;
using StbImageSharp;
using ImageInfo = Mini.Engine.DirectX.Resources.Surfaces.ImageInfo;
using Mini.Engine.Content.v2.Textures.Writers;
using Mini.Engine.Content.v2.Textures.Readers;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;
[Service]
public sealed class BrdfLutGenerator : IContentGenerator<TextureContent>
{
    private const int Resolution = 512;
    
    private readonly TextureGenerator TextureGenerator;
    private readonly Device Device;
    private readonly BrdfLutCompute Shader;
    private readonly BrdfLutCompute.User User;

    public BrdfLutGenerator(Device device, TextureGenerator textureGenerator, BrdfLutCompute shader)
    {
        this.Device = device;
        this.TextureGenerator = textureGenerator;        
        this.Shader = shader;
        this.User = this.Shader.CreateUserFor<BrdfLutGenerator>();
    }

    public string GeneratorKey => nameof(BrdfLutGenerator);

    public IContentCache CreateCache(IVirtualFileSystem fileSystem)
    {
        return new ContentCache<TextureContent>(this, fileSystem);
    }

    public void Generate(ContentId id, ContentRecord _, TrackingVirtualFileSystem __, ContentWriter contentWriter)
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
        var meta = new ContentRecord(new Content.Textures.TextureLoaderSettings(SuperCompressed.Mode.Linear, false));

        HdrTextureWriter.Write(contentWriter, meta, dependencies, image);
    }

    public TextureContent Load(ContentId id, ContentReader contentReader)
    {
        var header = contentReader.ReadHeader();
        var settings = new TextureLoaderSettings(SuperCompressed.Mode.Linear, false);
        var texture = HdrTextureReader.Read(this.Device, id, settings, contentReader);

        return new TextureContent(id, texture, new ContentRecord(settings), header.Dependencies);
    }

    public void Reload(Content.v2.IContent original, TrackingVirtualFileSystem fileSystem, Stream rwStream)
    {
        TextureReloader.Reload(this, (TextureContent)original, fileSystem, rwStream);
    }
}
