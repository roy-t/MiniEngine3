using Mini.Engine.DirectX;
using Mini.Engine.IO;
using SuperCompressed.BasisUniversal;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed class CompressedTextureLoader : IContentDataLoader<TextureData>, IDisposable
{
    private const Format SRgbFormat = Format.BC7_UNorm_SRgb;
    private const Format LinearFormat = Format.BC7_UNorm;

    private readonly IVirtualFileSystem FileSystem;
    private readonly Transcoder Transcoder;
    private readonly Encoder Encoder;

    public CompressedTextureLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
        this.Encoder = new Encoder();
        this.Transcoder = new Transcoder();
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSettings ? textureLoaderSettings : TextureLoaderSettings.Default;

        // TODO: do not encode and then transcode, us pre-encoded files
        var dfs = (DiskFileSystem)this.FileSystem;
        var path = dfs.ToAbsolute(id.Path);
        var data = this.Encoder.EncodeEtc1s(path, generateMipmaps: settings.ShouldMipMap, renormalize: settings.IsNormalized);

        //var data = this.FileSystem.ReadAllBytes(id.Path);

        var imageCount = this.Transcoder.GetImageCount(data);
        if (imageCount != 1)
        {
            throw new Exception($"Image file invalid it contains {imageCount} image(s)");
        }

        var mipMapCount = this.Transcoder.GetMipMapCount(data, 0);
        if (mipMapCount < 1)
        {
            throw new Exception($"Image invalid it contains {mipMapCount} mipmaps");
        }

        var mipmaps = new List<byte[]>(mipMapCount);
        var fullImage = this.Transcoder.Transcode(data, 0, 0, out var width, out var height, out var pitch);
        mipmaps.Add(fullImage);
        for (var i = 1; i < mipMapCount; i++)
        {
            mipmaps.Add(this.Transcoder.Transcode(data, 0, i, out var _, out var _, out var _));
        }

        var format = settings.IsSRgb ? SRgbFormat : LinearFormat;
        return new TextureData(id, width, height, pitch, format, false, mipmaps);
    }

    public void Dispose()
    {
        this.Transcoder.Dispose();
    }
}
