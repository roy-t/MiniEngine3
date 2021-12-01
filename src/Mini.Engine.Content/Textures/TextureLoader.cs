using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Serilog;
using StbImageSharp;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal class TextureLoader
{
    private readonly ILogger Logger;
    private const Format ByteFormat = Format.R8G8B8A8_UNorm_SRgb;
    private const Format FloatFormat = Format.R32G32B32A32_Float;

    public TextureLoader(ILogger logger)
    {
        this.Logger = logger.ForContext<TextureLoader>();
    }

    public TextureData<byte> Load(IVirtualFileSystem fileSystem, string fileName)
    {
        using var stream = fileSystem.OpenRead(fileName);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * ByteFormat.SizeOfInBytes();
        return new TextureData<byte>(fileName, image.Width, image.Height, pitch, ByteFormat, image.Data);
    }

    public TextureData<float> LoadFloat(IVirtualFileSystem fileSystem, string fileName)
    {
        using var stream = fileSystem.OpenRead(fileName);
        var image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * ByteFormat.SizeOfInBytes();
        return new TextureData<float>(fileName, image.Width, image.Height, pitch, FloatFormat, image.Data);
    }
}
