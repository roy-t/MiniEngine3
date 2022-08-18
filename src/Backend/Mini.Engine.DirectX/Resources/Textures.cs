using Mini.Engine.Core;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;


public readonly record struct ImageInfo(int Width, int Height, Format Format, int Pitch = 0, int ArraySize = 1);
public readonly record struct MipMapInfo(MipMapFlags Flags, int Levels)
{
    public static MipMapInfo Generated(int imageWidth) { return new MipMapInfo(MipMapFlags.Generated, Dimensions.MipSlices(imageWidth)); }
    public static MipMapInfo Provided(int levels) { return new MipMapInfo(MipMapFlags.Provided, levels); }
    public static MipMapInfo None() { return new MipMapInfo(MipMapFlags.None, 1); }   
}
public enum MipMapFlags { None, Provided, Generated };
public enum BindInfo { ShaderResource, RenderTarget, UnorderedAccessView, DepthStencil };
public enum ResourceInfo { Texture, Cube };

public static class Textures
{

    internal static ID3D11Texture2D Create(string user, string meaning, Device device, ImageInfo image, MipMapInfo mipMapInfo, BindInfo binding, ResourceInfo resource = ResourceInfo.Texture)
    {
        var description = CreateDescription(image, mipMapInfo, binding, resource);

        var texture = device.ID3D11Device.CreateTexture2D(description);
        texture.DebugName = DebugNameGenerator.GetName(user, "Texture2D", meaning, image.Format);

        return texture;
    }

    private static Texture2DDescription CreateDescription(ImageInfo image, MipMapInfo mipMapInfo, BindInfo binding, ResourceInfo resource)
    {
        var bindFlags = BindFlags.None;
        switch (binding)
        {
            case BindInfo.ShaderResource:
                bindFlags = BindFlags.ShaderResource;
                break;
            case BindInfo.RenderTarget:
                bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
                break;
            case BindInfo.UnorderedAccessView:
                bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget | BindFlags.UnorderedAccess;
                break;
            case BindInfo.DepthStencil:
                bindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil;
                break;            
        }

        var optionFlags = ResourceOptionFlags.None;
        switch (resource)
        {
            case ResourceInfo.Texture:
                optionFlags = ResourceOptionFlags.None;
                break;
            case ResourceInfo.Cube:
                optionFlags = ResourceOptionFlags.TextureCube;
                break;
        }

        var levels = 0;
        switch (mipMapInfo.Flags)
        {
            case MipMapFlags.None:
                levels = 1;
                break;
            case MipMapFlags.Provided:
                levels = mipMapInfo.Levels;
                break;
            case MipMapFlags.Generated:
                bindFlags |= BindFlags.RenderTarget;
                optionFlags |= ResourceOptionFlags.GenerateMips;
                levels = 0;
                break;
        }

        var description = new Texture2DDescription
        {
            Width = image.Width,
            Height = image.Height,
            Format = image.Format,
            MipLevels = levels,
            ArraySize = image.ArraySize,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = bindFlags,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = optionFlags
        };
        return description;
    }

    public static void SetPixels<T>(Device device, ID3D11Texture2D texture, ID3D11ShaderResourceView view, ImageInfo imageInfo, MipMapInfo mipMapInfo, ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        SetPixels(device, texture, imageInfo, pixels);

        if (mipMapInfo.Flags == MipMapFlags.Generated)
        {
            device.ID3D11DeviceContext.GenerateMips(view);
        }
    }

    public static void SetPixels<T>(Device device, ID3D11Texture2D texture, ID3D11ShaderResourceView view, ImageInfo imageInfo, MipMapInfo mipMapInfo, ReadOnlySpan<T> pixels, int mipSlice, int arraySlice)
       where T : unmanaged
    {
        var subresource = D3D11.CalculateSubResourceIndex(mipSlice, arraySlice, mipMapInfo.Levels);
        var pitch = (int)(imageInfo.Pitch / Math.Pow(2, mipSlice));
        device.ID3D11DeviceContext.UpdateSubresource(pixels, texture, subresource, pitch);

        if (mipMapInfo.Flags == MipMapFlags.Generated)
        {
            device.ID3D11DeviceContext.GenerateMips(view);
        }
    }

    public static void SetPixels<T>(Device device, ID3D11Texture2D texture, ImageInfo imageInfo, ReadOnlySpan<T> pixels, int mipMapIndex = 0)
        where T : unmanaged
    {
        var pitch = (int)(imageInfo.Pitch / Math.Pow(2, mipMapIndex));
        device.ID3D11DeviceContext.UpdateSubresource(pixels, texture, mipMapIndex, pitch);
    }
}
