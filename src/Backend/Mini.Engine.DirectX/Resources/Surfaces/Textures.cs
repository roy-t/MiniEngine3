using LibGame.Mathematics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public enum MipMapFlags { None, Provided, Generated };
public enum BindInfo { ShaderResource, RenderTarget, UnorderedAccessView, DepthStencil, Staging };
public enum ResourceInfo { Texture, Cube };

// TODO: is PitchOverride really used?
public readonly record struct ImageInfo(int DimX, int DimY, Format Format, int PitchOverride = 0, int DimZ = 1)
{
    public int Pitch => this.PitchOverride > 0
        ? this.PitchOverride
        : this.Format.BytesPerPixel() * this.DimX;

    public int Pixels => this.DimX * this.DimY * this.DimZ;

}
public readonly record struct MipMapInfo(MipMapFlags Flags, int Levels)
{
    public static MipMapInfo Generated(int imageWidth) { return new MipMapInfo(MipMapFlags.Generated, Dimensions.MipSlices(imageWidth)); }
    public static MipMapInfo Provided(int levels) { return new MipMapInfo(MipMapFlags.Provided, levels); }
    public static MipMapInfo None() { return new MipMapInfo(MipMapFlags.None, 1); }
}

public readonly record struct SamplingInfo(int Count, int Quality)
{
    // DirectX11 Hardware must support 1, 4, and 8 sample counts
    // https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_standard_multisample_quality_levels
    public const int MSAA_SAMPLE_COUNT = 8;

    public static readonly SamplingInfo None = new(1, 0);

    public static SamplingInfo GetMaximum(Device device, Format format)
    {
        var count = 1;
        var quality = 0;
        if (device.IsMultiSamplingSupported(format, MSAA_SAMPLE_COUNT))
        {
            count = MSAA_SAMPLE_COUNT;
            quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
        }


        return new SamplingInfo(count, quality);
    }

    public ShaderResourceViewDimension GetSrvDimensions(bool array = false)
    {
        if (this.Count > 1)
        {
            if (array)
            {
                return ShaderResourceViewDimension.Texture2DMultisampledArray;
            }
            return ShaderResourceViewDimension.Texture2DMultisampled;
        }

        if (array)
        {
            return ShaderResourceViewDimension.Texture2DArray;
        }
        return ShaderResourceViewDimension.Texture2D;
    }

    public RenderTargetViewDimension GetRtvDimensions(bool array = false)
    {
        if (this.Count > 1)
        {
            if (array)
            {
                return RenderTargetViewDimension.Texture2DMultisampledArray;
            }
            return RenderTargetViewDimension.Texture2DMultisampled;
        }

        if (array)
        {
            return RenderTargetViewDimension.Texture2DArray;
        }
        return RenderTargetViewDimension.Texture2D;
    }

    public DepthStencilViewDimension GetDsvDimensions(bool array = false)
    {
        if (this.Count > 1)
        {
            if (array)
            {
                return DepthStencilViewDimension.Texture2DMultisampledArray;
            }
            return DepthStencilViewDimension.Texture2DMultisampled;
        }

        if (array)
        {
            return DepthStencilViewDimension.Texture2DArray;
        }
        return DepthStencilViewDimension.Texture2D;
    }
}

public static class Textures
{
    internal static ID3D11Texture2D Create(Device device, string owner, ImageInfo image, MipMapInfo mipMapInfo, BindInfo binding, ResourceInfo resource = ResourceInfo.Texture)
    {
        return Create(device, owner, image, mipMapInfo, binding, SamplingInfo.None, resource);
    }

    internal static ID3D11Texture2D Create(Device device, string owner, ImageInfo image, MipMapInfo mipMapInfo, BindInfo binding, SamplingInfo multiSample, ResourceInfo resource = ResourceInfo.Texture)
    {
        var description = CreateDescription(image, mipMapInfo, binding, resource, multiSample);

        var texture = device.ID3D11Device.CreateTexture2D(description);
        texture.DebugName = DebugNameGenerator.GetName(owner, "Texture2D", image.Format);

        return texture;
    }

    internal static Texture2DDescription CreateDescription(ImageInfo image, MipMapInfo mipMapInfo, BindInfo binding, ResourceInfo resource, SamplingInfo multiSample)
    {
        var bindFlags = BindFlags.None;
        var cpuAccess = CpuAccessFlags.None;
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
            case BindInfo.Staging:
                cpuAccess = CpuAccessFlags.Read;
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

        var usage = ResourceUsage.Default;
        if (binding == BindInfo.Staging)
        {
            usage = ResourceUsage.Staging;
        }

        var sample = new SampleDescription(multiSample.Count, multiSample.Quality);

        var description = new Texture2DDescription
        {
            Width = image.DimX,
            Height = image.DimY,
            Format = image.Format,
            MipLevels = levels,
            ArraySize = image.DimZ,
            SampleDescription = sample,
            Usage = usage,
            BindFlags = bindFlags,
            CPUAccessFlags = cpuAccess,
            MiscFlags = optionFlags
        };
        return description;
    }

    public static void SetPixels<T>(Device device, ID3D11Texture2D texture, ID3D11ShaderResourceView view, ImageInfo imageInfo, MipMapInfo mipMapInfo, ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        SetPixels(device, texture, view, imageInfo, mipMapInfo, pixels, 0, 0);
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
}
