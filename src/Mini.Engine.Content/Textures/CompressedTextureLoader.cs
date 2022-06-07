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
        var transcodedData = this.Transcoder.Transcode(data, out var width, out var height, out var pitch);

        // TODO: support mipmapping for compressed textures by loading the generated mipmaps
        return new TextureData(id, width, height, pitch, SRgbFormat, false, transcodedData);
    }

    public void Dispose()
    {
        this.Transcoder.Dispose();
    }
}
