using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures.Readers;
public static class CompressedTextureReader
{
    public static (TextureLoaderSettings, ITexture) Read(Device device, ContentId id, TranscodeFormats transcodeFormat, ContentReader reader)
    {        
        var settings = reader.ReadTextureSettings();
        var image = reader.ReadArray();

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

        using var trancoded = Transcoder.Instance.Transcode(image, 0, 0, transcodeFormat);
        var width = trancoded.Width;
        var heigth = trancoded.Heigth;
        var pitch = trancoded.Pitch;
        var format = FormatSelector.SelectCompressedFormat(settings.Mode, transcodeFormat);

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

        var texture = new Texture(device, id.ToString(), imageInfo, mipMapInfo);
        texture.SetPixels<byte>(device, trancoded.Data);

        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            for (var i = 1; i < mipMapCount; i++)
            {
                using var transcodedMipMap = Transcoder.Instance.Transcode(image, 0, i, transcodeFormat);
                texture.SetPixels<byte>(device, transcodedMipMap.Data, i, 0);
            }
        }

        return (settings, texture);
    }
}
