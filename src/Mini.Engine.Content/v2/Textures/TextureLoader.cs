using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using SuperCompressed;

using DXR = Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.v2.Textures;

internal class TextureLoader : IContentGenerator<TextureContent>
{
    private const int MinBlockSize = 4;

    private static readonly Guid HeaderCompressed = new("{650531A2-0DF5-4E36-85AB-2AC5A01DBEA2}");
    private static readonly Guid HeaderUncompressed = new("{650531A2-0DF5-4E36-85AB-A0850356195E}");
    private static readonly Guid HeaderHdr = new("{650531A2-0DF5-4E36-85AB-F5525464F962}");

    //private readonly ResourceManager Resources;

    // TODO: create path for HDRTextures

    public void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, Stream stream)
    {
        if (!HasSupportedExtension(id))
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }

        var bytes = fileSystem.ReadAllBytes(id.Path);
        var image = Image.FromMemory(bytes);
        
        var header = (image.Width < MinBlockSize || image.Height < MinBlockSize)
            ? HeaderUncompressed
            : HeaderCompressed;

        var encoded = Encoder.Instance.Encode(image, meta.TextureSettings.Mode, MipMapGeneration.Lanczos3, Quality.Default);
        ContentWriter.WriteAll(stream, header, meta, fileSystem.GetDependencies(), encoded);
    }

    public TextureContent Load(Device device, ContentId id, ContentBlob blob)
    {
        if (blob.Header == HeaderCompressed)
        {
            return Load(device, id, blob, TranscodeFormats.BC7_RGBA);
        }

        if (blob.Header == HeaderUncompressed)
        {
            return Load(device, id, blob, TranscodeFormats.RGBA32);
        }

        if (blob.Header == HeaderHdr)
        {
            return LoadHdr(device, id, blob);
        }

        throw new ArgumentException($"Unexpected header, {nameof(blob)} is not a preprocessed texture", nameof(blob));
    }

    private TextureContent Load(Device device, ContentId id, ContentBlob blob, TranscodeFormats transcodeFormat)
    {
        var settings = blob.Meta.TextureSettings;
        var image = blob.Contents;

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

        var texture = DXR.Textures.Create(device, id.ToString(), imageInfo, mipMapInfo, BindInfo.ShaderResource);
        var view = DXR.ShaderResourceViews.Create(device, texture, id.ToString(), imageInfo);

        DXR.Textures.SetPixels<byte>(device, texture, view, imageInfo, mipMapInfo, trancoded.Data);

        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            for (var i = 1; i < mipMapCount; i++)
            {
                using var transcodedMipMap = Transcoder.Instance.Transcode(image, 0, i, transcodeFormat);
                DXR.Textures.SetPixels<byte>(device, texture, view, imageInfo, mipMapInfo, transcodedMipMap.Data, i, 0);
            }
        }

        var data = new TextureData(id, imageInfo, mipMapInfo, texture, view);
        return new TextureContent(id, data, blob.Dependencies);
    }

    private TextureContent LoadUncompressed(Device device, ContentId id, ContentBlob blob)
    {
        throw new NotImplementedException();
    }

    private TextureContent LoadHdr(Device device, ContentId id, ContentBlob blob)
    {
        throw new NotImplementedException();
    }

    private static bool HasSupportedExtension(ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => true,
            _ => false
        };
    }
}

