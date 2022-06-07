﻿using Mini.Engine.Core;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public readonly record struct ImageInfo(int Width, int Height, Format Format, int Pitch, int ArraySize = 1);
public readonly record struct MipMapInfo(MipMapFlags Flags, int Levels)
{
    public static MipMapInfo Generated(int imageWidth) { return new MipMapInfo(MipMapFlags.Generated, Dimensions.MipSlices(imageWidth)); }
    public static MipMapInfo Provided(int levels) { return new MipMapInfo(MipMapFlags.Provided, levels); }
    public static MipMapInfo None() { return new MipMapInfo(MipMapFlags.None, 1); }
    public static MipMapInfo Compute(bool generateMipMaps, int imageWidth, int levels)
    {
        if (generateMipMaps) { return Generated(imageWidth); }
        else if (levels > 1) { return Provided(levels); }
        return None();
    }
}
public enum MipMapFlags { None, Provided, Generated };
public enum BindInfo { ShaderResource, RenderTargetShaderResource, DepthStencilShaderResource };
public enum ResourceInfo { Texture, Cube };

public static class Textures
{
    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, string user, string meaning)
    {
        return Create(device, width, height, format, 1, false, user, meaning);
    }

    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, int mipmapSlizes, bool generateMipMaps, string user, string meaning)
    {
        return Create(device, width, height, format, 1, mipmapSlizes, generateMipMaps, user, meaning);
    }

    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, int arraySize, int mipmapSlizes, bool generateMipMaps, string user, string meaning)
    {
        return Create(device, width, height, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.None, arraySize, mipmapSlizes, generateMipMaps, user, meaning);
    }

    internal static ID3D11Texture2D Create(Device device, int width, int height, Format format, BindFlags bindFlags, ResourceOptionFlags optionFlags, int arraySize, int mipmapSlizes, bool generateMipMaps, string user, string meaning)
    {
        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = generateMipMaps ? 0 : mipmapSlizes,
            ArraySize = arraySize,
            Format = format,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = (generateMipMaps ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.None) | bindFlags,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = (generateMipMaps ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None) | optionFlags
        };

        var texture = device.ID3D11Device.CreateTexture2D(description);
        texture.DebugName = DebugNameGenerator.GetName(user, "Texture2D", meaning, format);

        return texture;
    }

    internal static ID3D11Texture2D Create(string user, string meaning, Device device, ImageInfo image, MipMapInfo mipMapInfo, BindInfo binding, ResourceInfo resource = ResourceInfo.Texture)
    {
        var bindFlags = BindFlags.None;
        switch (binding)
        {
            case BindInfo.ShaderResource:
                bindFlags = BindFlags.ShaderResource;
                break;
            case BindInfo.RenderTargetShaderResource:
                bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
                break;
            case BindInfo.DepthStencilShaderResource:
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
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = optionFlags
        };

        var texture = device.ID3D11Device.CreateTexture2D(description);
        texture.DebugName = DebugNameGenerator.GetName(user, "Texture2D", meaning, image.Format);

        return texture;
    }
}
