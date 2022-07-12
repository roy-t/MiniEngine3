using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using SuperCompressed;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed class CompressedTextureLoader : IContentDataLoader<TextureData>
{
    private const Format SRgbFormat = Format.BC7_UNorm_SRgb;
    private const Format LinearFormat = Format.BC7_UNorm;

    private readonly IVirtualFileSystem FileSystem;

    public CompressedTextureLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSettings ? textureLoaderSettings : TextureLoaderSettings.Default;

        // TODO: do not encode and then transcode, us pre-encoded files        

        var dfs = (DiskFileSystem)this.FileSystem;
        var path = dfs.ToAbsolute(id.Path);

        var image = Image.FromStream(File.OpenRead(path));

        // TODO: mipmapping and srgb stuff depends on image type
        var data = Encoder.Instance.Encode(image, Mode.SRgb, MipMapGeneration.Full, Quality.Default);        

        var imageCount = Transcoder.Instance.GetImageCount(data);
        if (imageCount != 1)
        {
            throw new Exception($"Image file invalid it contains {imageCount} image(s)");
        }

        var mipMapCount = Transcoder.Instance.GetLevelCount(data, 0);
        if (mipMapCount < 1)
        {
            throw new Exception($"Image invalid it contains {mipMapCount} mipmaps");
        }

        var mipmaps = new List<byte[]>(mipMapCount);

        using var trancoded = Transcoder.Instance.Transcode(data, 0, 0, TranscodeFormats.BC7_RGBA);
        var width = trancoded.Width;
        var heigth = trancoded.Heigth;
        var pitch = trancoded.Pitch;

        var texture = new byte[trancoded.Data.Length];
        trancoded.Data.CopyTo(texture);

        mipmaps.Add(texture);
        
        for (var i = 1; i < mipMapCount; i++)
        {
            using var transcodedMipMap = Transcoder.Instance.Transcode(data, 0, i, TranscodeFormats.BC7_RGBA);
            var mipMap  = new byte[transcodedMipMap.Data.Length];
            transcodedMipMap.Data.CopyTo(mipMap);

            mipmaps.Add(mipMap);
        }

        var format = SRgbFormat;//: LinearFormat;
        var imageInfo = new ImageInfo(width, heigth, format, pitch);

        var mipmapInfo = MipMapInfo.None();
        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            mipmapInfo = MipMapInfo.Provided(mipMapCount);
        }        
                
        return new TextureData(id, imageInfo, mipmapInfo, mipmaps);
    }  
}
