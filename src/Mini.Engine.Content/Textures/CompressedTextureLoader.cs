using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using SuperCompressed;
using Vortice.DXGI;
using DXR = Mini.Engine.DirectX.Resources;

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

        // BEGIN HACK
        // TODO: do not encode and then transcode, us pre-encoded files        

        var dfs = (DiskFileSystem)this.FileSystem;
        var path = dfs.ToAbsolute(id.Path);

        using var stream = File.OpenRead(path);
        var image = Image.FromStream(stream);        
        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Full, Quality.Default);

        // END HACK

        var imageCount = Transcoder.Instance.GetImageCount(encoded);
        if (imageCount != 1)
        {
            throw new Exception($"Image file invalid it contains {imageCount} image(s)");
        }

        var mipMapCount = Transcoder.Instance.GetLevelCount(encoded, 0);
        if (mipMapCount < 1)
        {
            throw new Exception($"Image invalid it contains {mipMapCount} mipmaps");
        }
       
        using var trancoded = Transcoder.Instance.Transcode(encoded, 0, 0, TranscodeFormats.BC7_RGBA);
        var width = trancoded.Width;
        var heigth = trancoded.Heigth;
        var pitch = trancoded.Pitch;
        
        var format = settings.Mode switch
        {
            Mode.Linear or Mode.Normalized => LinearFormat,
            Mode.SRgb => SRgbFormat,
            _ => throw new ArgumentOutOfRangeException(nameof(loaderSettings))
        };

        var imageInfo = new ImageInfo(width, heigth, format, pitch);

        var mipMapInfo = MipMapInfo.None();
        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            mipMapInfo = MipMapInfo.Provided(mipMapCount);
        }

        if (settings.ShouldMipMap && mipMapCount == 1)
        {
            mipMapInfo = MipMapInfo.Generated(width);
        }

        var texture = DXR.Textures.Create(id.ToString(), string.Empty, device, imageInfo, mipMapInfo, BindInfo.ShaderResource);
        var view = DXR.ShaderResourceViews.Create(device, texture, format, id.ToString(), string.Empty);

        DXR.Textures.SetPixels<byte>(device, texture, view, imageInfo, mipMapInfo, trancoded.Data);

        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            for (var i = 1; i < mipMapCount; i++)
            {
                using var transcodedMipMap = Transcoder.Instance.Transcode(encoded, 0, i, TranscodeFormats.BC7_RGBA);

                DXR.Textures.SetPixels<byte>(device, texture, imageInfo, transcodedMipMap.Data, i);
            }
        }

        return new TextureData(id, imageInfo, mipMapInfo, texture, view);
    }
}
