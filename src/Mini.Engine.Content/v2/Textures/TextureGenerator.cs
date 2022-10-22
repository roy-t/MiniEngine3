using Mini.Engine.Configuration;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using SuperCompressed;
using Stb = StbImageSharp;

namespace Mini.Engine.Content.v2.Textures;

[Service]
public class TextureGenerator : IContentGenerator<TextureContent>
{
    private const int MinBlockSize = 4;

    private static readonly Guid HeaderCompressed = new("{650531A2-0DF5-4E36-85AB-2AC5A01DBEA2}");
    private static readonly Guid HeaderUncompressed = new("{650531A2-0DF5-4E36-85AB-A0850356195E}");
    private static readonly Guid HeaderHdr = new("{650531A2-0DF5-4E36-85AB-F5525464F962}");

    private readonly Device Device;

    public TextureGenerator(Device device)
    {
        this.Device = device;
    }

    public string GeneratorKey => nameof(TextureGenerator);

    public void Reload(IContent original, TrackingVirtualFileSystem fileSystem, Stream rwStream)
    {
        var wrapper = (TextureContent)original;

        using var writer = new ContentWriter(rwStream);
        this.Generate(original.Id, wrapper.Meta, fileSystem, writer);

        rwStream.Seek(0, SeekOrigin.Begin);

        using var reader = new ContentReader(rwStream);
        var texture = this.Load(wrapper.Id, reader);

        wrapper.Reload(texture);
    }

    public void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, ContentWriter contentWriter)
    {
        if (HasSupportedSdrExtension(id))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Image.FromMemory(bytes);

            var header = (image.Width < MinBlockSize || image.Height < MinBlockSize)
                ? HeaderUncompressed
                : HeaderCompressed;

            var encoded = Encoder.Instance.Encode(image, meta.TextureSettings.Mode, MipMapGeneration.Lanczos3, Quality.Default);
            contentWriter.WriteCommon(header, meta, fileSystem.GetDependencies(), encoded);
        }
        else if (HasSupportedHdrExtension(id))
        {
            var bytes = fileSystem.ReadAllBytes(id.Path);
            var image = Stb.ImageResultFloat.FromMemory(bytes, Stb.ColorComponents.RedGreenBlue);
            var floats = image.Data;
            unsafe
            {
                fixed (float* ptr = floats)
                {
                    var data = new ReadOnlySpan<byte>(ptr, image.Data.Length * 4);
                    contentWriter.WriteCommon(HeaderHdr, meta, fileSystem.GetDependencies(), data);
                    contentWriter.Writer.Write(image.Width);
                    contentWriter.Writer.Write(image.Height);
                }
            }
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    public TextureContent Load(ContentId id, ContentReader reader)
    {
        var blob = reader.ReadCommon();

        ITexture texture;
        if (blob.Header == HeaderCompressed)
        {
            texture = this.LoadData(id, blob, TranscodeFormats.BC7_RGBA);
        }
        else if (blob.Header == HeaderUncompressed)
        {
            texture = this.LoadData(id, blob, TranscodeFormats.RGBA32);
        }
        else if (blob.Header == HeaderHdr)
        {
            var width = reader.Reader.ReadInt32();
            var heigth = reader.Reader.ReadInt32();
            texture = this.LoadHdrData(id, blob, width, heigth);
        }
        else
        {
            throw new NotSupportedException($"Unexpected header: {blob.Header}");
        }

        return new TextureContent(id, texture, blob.Meta, this.GeneratorKey, blob.Dependencies);
    }

    private ITexture LoadData(ContentId id, ContentBlob blob, TranscodeFormats transcodeFormat)
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

        var texture = new Texture(this.Device, id.ToString(), imageInfo, mipMapInfo);
        texture.SetPixels<byte>(this.Device, trancoded.Data);

        if (settings.ShouldMipMap && mipMapCount > 1)
        {
            for (var i = 1; i < mipMapCount; i++)
            {
                using var transcodedMipMap = Transcoder.Instance.Transcode(image, 0, i, transcodeFormat);
                texture.SetPixels<byte>(this.Device, transcodedMipMap.Data, i, 0);
            }
        }

        return texture;
    }

    private ITexture LoadHdrData(ContentId id, ContentBlob blob, int width, int heigth)
    {
        var settings = blob.Meta.TextureSettings;
        var data = blob.Contents;
        unsafe
        {
            fixed (byte* ptr = data)
            {
                var image = new ReadOnlySpan<float>(ptr, data.Length / 4);
                var format = FormatSelector.SelectHDRFormat(settings.Mode, 3);

                var pitch = width * format.BytesPerPixel();

                var imageInfo = new ImageInfo(width, heigth, format, pitch);
                var mipMapInfo = MipMapInfo.None();
                if (settings.ShouldMipMap)
                {
                    mipMapInfo = MipMapInfo.Generated(width);
                }

                var texture = new Texture(this.Device, id.ToString(), imageInfo, mipMapInfo);
                texture.SetPixels(this.Device, image);

                return texture;
            }
        }
    }

    private static bool HasSupportedSdrExtension(ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tga" or ".psd" or ".gif" => true,
            _ => false
        };
    }

    private static bool HasSupportedHdrExtension(ContentId id)
    {
        var extension = Path.GetExtension(id.Path).ToLowerInvariant();
        return extension switch
        {
            ".hdr" => true,
            _ => false
        };
    }
}

