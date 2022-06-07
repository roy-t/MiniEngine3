using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
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

    public ITexture2D WhitePixel => this.CreatePixel(Colors.White, "White");

    public ITexture2D BlackPixel => this.CreatePixel(Colors.Black, "Black");

    public ITexture2D RedPixel => this.CreatePixel(Colors.Red, "Red");

    public ITexture2D GreenPixel => this.CreatePixel(Colors.Green, "Green");

    public ITexture2D BluePixel => this.CreatePixel(Colors.Blue, "Blue");

    public ITexture2D ConductivePixel => this.CreatePixel(Colors.White, "Metalicness");

    public ITexture2D DielectricPixel => this.CreatePixel(Colors.Black, "Metalicness");

    public ITexture2D RoughPixel => this.RoughnessPixel(1.0f);

    public ITexture2D SmoothPixel => this.RoughnessPixel(0.0f);

    public ITexture2D VisiblePixel => this.AmbientOcclussionPixel(1.0f);

    public ITexture2D OccludedPixel => this.AmbientOcclussionPixel(0.0f);

    public ITexture2D AlbedoPixel(Color4 color)
    {
        return this.CreatePixel(color, "Albedo");
    }

    public ITexture2D NormalPixel()
    {
        return this.NormalPixel(Vector3.UnitZ);
    }

    public ITexture2D NormalPixel(Vector3 direction)
    {
        return this.CreatePixel(new Color4(Pack(direction), 1.0f), "Normal");
    }

    public ITexture2D MetalicnessPixel(float metalicness)
    {
        return this.CreatePixel(new Color4(metalicness, metalicness, metalicness), "Metalicness");
    }

    public ITexture2D RoughnessPixel(float roughness)
    {
        return this.CreatePixel(new Color4(roughness, roughness, roughness), "Roughness");
    }

    public ITexture2D AmbientOcclussionPixel(float ao)
    {
        return this.CreatePixel(new Color4(ao, ao, ao), "AmbientOcclusion");
    }

    private ITexture2D CreatePixel(Color4 color, string meaning)
    {
        var image = new ImageInfo(1, 1, Format.R16G16B16A16_Float, 1 * Format.R16G16B16A16_Float.SizeOfInBytes());
        var pixel = new Texture2D(this.Device, image, MipMapInfo.None(), nameof(Pixels), meaning);
        pixel.SetPixels(this.Device, new Span<Color4>(new Color4[] { color }));

        this.Content.Link(pixel, $"pixels/{meaning}");

        return pixel;
    }

    private static Vector3 Pack(Vector3 direction)
    {
        return 0.5f * (Vector3.Normalize(direction) + Vector3.One);
    }
}