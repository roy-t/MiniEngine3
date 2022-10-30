using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.v2.Textures.Readers;
public static class HdrTextureReader
{
    public static (TextureLoaderSettings, ITexture) Read(Device device, ContentId id, ContentReader reader)
    {        
        var settings = reader.ReadTextureSettings();
        var components = reader.Reader.ReadInt32();
        var width = reader.Reader.ReadInt32();
        var heigth = reader.Reader.ReadInt32();
        var data = reader.ReadArray();
        unsafe
        {
            fixed (byte* ptr = data)
            {
                var image = new ReadOnlySpan<float>(ptr, data.Length / 4);
                var format = FormatSelector.SelectHDRFormat(settings.Mode, components);

                var pitch = width * format.BytesPerPixel();

                var imageInfo = new ImageInfo(width, heigth, format, pitch);
                var mipMapInfo = MipMapInfo.None();
                if (settings.ShouldMipMap)
                {
                    mipMapInfo = MipMapInfo.Generated(width);
                }

                var texture = new Texture(device, id.ToString(), imageInfo, mipMapInfo);
                texture.SetPixels(device, image);

                return (settings, texture);
            }
        }
    }
}
