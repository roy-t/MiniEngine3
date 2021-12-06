using System;
using Mini.Engine.IO;
using StbImageSharp;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

public sealed class HdrTextureDataLoader : IContentDataLoader<TextureData>
{
    private const Format HdrFormat = Format.R32G32B32A32_Float;
    private static readonly int FormatSizeInBytes = HdrFormat.SizeOfInBytes();

    private readonly IVirtualFileSystem FileSystem;

    public HdrTextureDataLoader(IVirtualFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    public TextureData Load(string fileName)
    {
        using var stream = this.FileSystem.OpenRead(fileName);
        var image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var pitch = image.Width * FormatSizeInBytes;

        var bytes = new byte[FormatSizeInBytes * image.Data.Length];
        Buffer.BlockCopy(image.Data, 0, bytes, 0, bytes.Length);

        return new TextureData(fileName, image.Width, image.Height, pitch, HdrFormat, bytes);
    }
}
