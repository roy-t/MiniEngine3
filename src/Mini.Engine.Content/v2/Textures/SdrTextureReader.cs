using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures;
public static class SdrTextureReader
{
    private const int MinBlockSize = 4;

    public static ITexture Read(Device device, ContentId id, ContentReader reader, TextureLoaderSettings settings, TranscodeFormats preferredFormat)
    {        
        var imageWidth = reader.Reader.ReadInt32();
        var imageHeigth = reader.Reader.ReadInt32();
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

        var targetFormat = GetSupportedFormat(preferredFormat, imageWidth, imageHeigth);
        using var trancoded = Transcoder.Instance.Transcode(image, 0, 0, targetFormat);
        var width = trancoded.Width;
        var heigth = trancoded.Heigth;
        var pitch = trancoded.Pitch;
        var format = FormatSelector.SelectCompressedFormat(settings.Mode, trancoded.Format);

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
                using var transcodedMipMap = Transcoder.Instance.Transcode(image, 0, i, targetFormat);
                texture.SetPixels<byte>(device, transcodedMipMap.Data, i, 0);
            }
        }

        return texture;
    }

    private static TranscodeFormats GetSupportedFormat(TranscodeFormats preferredFormat, int width, int heigth)
    {
        switch (preferredFormat)
        {
            case TranscodeFormats.ETC1_RGB:
            case TranscodeFormats.ETC2_RGBA:
            case TranscodeFormats.BC1_RGB:
            case TranscodeFormats.BC3_RGBA:
            case TranscodeFormats.BC4_R:
            case TranscodeFormats.BC5_RG:
            case TranscodeFormats.BC7_RGBA:
            case TranscodeFormats.PVRTC1_4_RGB:
            case TranscodeFormats.PVRTC1_4_RGBA:
            case TranscodeFormats.ASTC_4x4_RGBA:
            case TranscodeFormats.ETC2_EAC_R11:
            case TranscodeFormats.ETC2_EAC_RG11:
                if (width < MinBlockSize || heigth < MinBlockSize)
                {
                    return TranscodeFormats.RGBA32;
                }
                return preferredFormat;
            case TranscodeFormats.RGBA32:
            case TranscodeFormats.RGB565:
            case TranscodeFormats.BGR565:
            case TranscodeFormats.RGBA4444:
                return preferredFormat;
            default:
                throw new ArgumentOutOfRangeException(nameof(preferredFormat));
        }
    }
}
