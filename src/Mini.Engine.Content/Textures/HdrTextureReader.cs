using Mini.Engine.Content.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Content.Textures;
public static class HdrTextureReader
{
    public static ITexture Read(Device device, ContentId id, ContentReader reader, TextureSettings settings)
    {
        var components = reader.ReadInt();
        var width = reader.ReadInt();
        var heigth = reader.ReadInt();
        var data = reader.ReadBytes();
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

                return texture;
            }
        }
    }
}
