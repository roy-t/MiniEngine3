using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using SuperCompressed;
using DXR = Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.Textures;

internal sealed class CompressedTextureLoader : IContentDataLoader<TextureData>
{
    private readonly IVirtualFileSystem FileSystem;
    private readonly TextureCompressor Compressor;

    public CompressedTextureLoader(IVirtualFileSystem fileSystem, TextureCompressor compressor)
    {
        this.FileSystem = fileSystem;
        this.Compressor = compressor;
    }

    public TextureData Load(Device device, ContentId id, ILoaderSettings loaderSettings)
    {
        var settings = loaderSettings is TextureLoaderSettings textureLoaderSettings ? textureLoaderSettings : TextureLoaderSettings.Default;

        if (!this.FileSystem.Exists(id.Path))
        {
            this.Compressor.CompressSourceFileFor(id, settings);
        }

        var image = this.FileSystem.ReadAllBytes(id.Path);

        var imageCount = Transcoder.Instance.GetImageCount(image);
        if (imageCount != 1)
        {
            throw new Exception($"Image file invalid it contains {imageCount} image(s)");
        }

        var mipMapCount = Transcoder.Instance.GetLevelCount(image, 0);
        if (mipMapCount < 1)
        {
            throw new Exception($"Image invalid it contains {mipMapCount} mipmaps");
        }
       
        using var trancoded = Transcoder.Instance.Transcode(image, 0, 0, TranscodeFormats.BC7_RGBA);
        var width = trancoded.Width;
        var heigth = trancoded.Heigth;
        var pitch = trancoded.Pitch;
        var format = FormatSelector.SelectCompressedFormat(settings.Mode, TranscodeFormats.BC7_RGBA);     

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

        var texture = DXR.Textures.Create(device, id.ToString(), imageInfo, mipMapInfo, BindInfo.ShaderResource);
        var view = DXR.ShaderResourceViews.Create(device, texture, id.ToString(), imageInfo);

        DXR.Textures.SetPixels<byte>(device, texture, view, imageInfo, mipMapInfo, trancoded.Data);

        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            for (var i = 1; i < mipMapCount; i++)
            {
                using var transcodedMipMap = Transcoder.Instance.Transcode(image, 0, i, TranscodeFormats.BC7_RGBA);
                DXR.Textures.SetPixels<byte>(device, texture, view, imageInfo, mipMapInfo, transcodedMipMap.Data, i, 0);
            }
        }

        return new TextureData(id, imageInfo, mipMapInfo, texture, view);
    }
}
