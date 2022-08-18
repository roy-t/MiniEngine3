using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.vNext;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Textures;

[Content]
public sealed class Pixels
{
    private readonly Device Device;
    private readonly ContentManager Content;

    public Pixels(Device device, ContentManager content)
    {
        this.Device = device;
        this.Content = content;
    }

    public ITexture WhitePixel => this.CreatePixel(Colors.White, "White");

    public ITexture BlackPixel => this.CreatePixel(Colors.Black, "Black");

    public ITexture RedPixel => this.CreatePixel(Colors.Red, "Red");

    public ITexture GreenPixel => this.CreatePixel(Colors.Green, "Green");

    public ITexture BluePixel => this.CreatePixel(Colors.Blue, "Blue");

    public ITexture ConductivePixel => this.CreatePixel(Colors.White, "Metalicness");

    public ITexture DielectricPixel => this.CreatePixel(Colors.Black, "Metalicness");

    public ITexture RoughPixel => this.RoughnessPixel(1.0f);

    public ITexture SmoothPixel => this.RoughnessPixel(0.0f);

    public ITexture VisiblePixel => this.AmbientOcclussionPixel(1.0f);

    public ITexture OccludedPixel => this.AmbientOcclussionPixel(0.0f);

    public ITexture AlbedoPixel(Color4 color)
    {
        return this.CreatePixel(color, "Albedo");
    }

    public ITexture NormalPixel()
    {
        return this.NormalPixel(Vector3.UnitZ);
    }

    public ITexture NormalPixel(Vector3 direction)
    {
        return this.CreatePixel(new Color4(Pack(direction), 1.0f), "Normal");
    }

    public ITexture MetalicnessPixel(float metalicness)
    {
        return this.CreatePixel(new Color4(metalicness, metalicness, metalicness), "Metalicness");
    }

    public ITexture RoughnessPixel(float roughness)
    {
        return this.CreatePixel(new Color4(roughness, roughness, roughness), "Roughness");
    }

    public ITexture AmbientOcclussionPixel(float ao)
    {
        return this.CreatePixel(new Color4(ao, ao, ao), "AmbientOcclusion");
    }

    private ITexture CreatePixel(Color4 color, string meaning)
    {
        var image = new DirectX.Resources.ImageInfo(1, 1, Format.R16G16B16A16_Float, 1 * Format.R16G16B16A16_Float.BytesPerPixel());
        var mipMap = DirectX.Resources.MipMapInfo.None();
        var pixel = new Texture(this.Device, image, mipMap, nameof(Pixels) + "_meaning");
        pixel.SetPixels(this.Device, new ReadOnlySpan<Color4>(new Color4[] { color }));        

        this.Content.Link(pixel, $"pixels/{meaning}");

        return pixel;
    }

    private static Vector3 Pack(Vector3 direction)
    {
        return 0.5f * (Vector3.Normalize(direction) + Vector3.One);
    }
}