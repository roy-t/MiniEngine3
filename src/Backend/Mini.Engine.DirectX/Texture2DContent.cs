using System;
using Mini.Engine.IO;
using Vortice.DXGI;

namespace Mini.Engine.DirectX;

public sealed record TextureData<T>(string FileName, int Width, int Height, int Pitch, Format Format, T[] Data)
    where T : unmanaged;

public interface ITextureLoader
{
    public const Format ByteFormat = Format.R8G8B8A8_UNorm_SRgb;
    public const Format FloatFormat = Format.R32G32B32A32_Float;

    TextureData<byte> Load(IVirtualFileSystem fileSystem, string fileName);
    TextureData<float> LoadFloat(IVirtualFileSystem fileSystem, string fileName);
}

public sealed class Texture2DContent<T> : Texture2D, IContent
    where T : unmanaged
{
    private readonly ITextureLoader Loader;

    public Texture2DContent(Device device, ITextureLoader loader, TextureData<T> data, bool generateMipMaps = false, string name = "")
        : base(device, data.Width, data.Height, data.Format, generateMipMaps, name)
    {
        this.Loader = loader;
        this.FileName = data.FileName;

        this.Width = data.Width;
        this.Height = data.Height;
        this.Format = data.Format;

        device.ID3D11DeviceContext.UpdateSubresource(data.Data, this.Texture, 0, data.Pitch, 0);
    }

    public string FileName { get; }
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }

    public void Reload(Device device, IVirtualFileSystem fileSystem)
    {
        if (this.Format == ITextureLoader.ByteFormat)
        {
            var data = this.Loader.Load(fileSystem, this.FileName);
            this.Reload(device, data.Width, data.Height, data.Pitch, data.Format, data.Data);

        }
        else if (this.Format == ITextureLoader.FloatFormat)
        {
            var data = this.Loader.LoadFloat(fileSystem, this.FileName);
            this.Reload(device, data.Width, data.Height, data.Pitch, data.Format, data.Data);
        }
        else
        {
            throw new NotSupportedException($"Format {this.Format} is not supported");
        }
    }

    private void Reload<E>(Device device, int width, int height, int pitch, Format format, E[] data)
        where E : unmanaged
    {
        if (width != this.Width || height != this.Height || format != this.Format)
        {
            throw new NotSupportedException($"Cannot reload {this.FileName}, dimensions or format have changed");
        }
        device.ID3D11DeviceContext.UpdateSubresource(data, this.Texture, 0, pitch, 0);
    }
}
