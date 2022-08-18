using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
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

    public ISurface WhitePixel => this.CreatePixel(Colors.White, "White");

    public ISurface BlackPixel => this.CreatePixel(Colors.Black, "Black");

    public ISurface RedPixel => this.CreatePixel(Colors.Red, "Red");

    public ISurface GreenPixel => this.CreatePixel(Colors.Green, "Green");

    public ISurface BluePixel => this.CreatePixel(Colors.Blue, "Blue");

    public ISurface ConductivePixel => this.CreatePixel(Colors.White, "Metalicness");

    public ISurface DielectricPixel => this.CreatePixel(Colors.Black, "Metalicness");

    public ISurface RoughPixel => this.RoughnessPixel(1.0f);

    public ISurface SmoothPixel => this.RoughnessPixel(0.0f);

    public ISurface VisiblePixel => this.AmbientOcclussionPixel(1.0f);

    public ISurface OccludedPixel => this.AmbientOcclussionPixel(0.0f);

    public ISurface AlbedoPixel(Color4 color)
    {
        return this.CreatePixel(color, "Albedo");
    }

    public ISurface NormalPixel()
    {
        return this.NormalPixel(Vector3.UnitZ);
    }

    public ISurface NormalPixel(Vector3 direction)
    {
        return this.CreatePixel(new Color4(Pack(direction), 1.0f), "Normal");
    }

    public ISurface MetalicnessPixel(float metalicness)
    {
        return this.CreatePixel(new Color4(metalicness, metalicness, metalicness), "Metalicness");
    }

    public ISurface RoughnessPixel(float roughness)
    {
        return this.CreatePixel(new Color4(roughness, roughness, roughness), "Roughness");
    }

    public ISurface AmbientOcclussionPixel(float ao)
    {
        return this.CreatePixel(new Color4(ao, ao, ao), "AmbientOcclusion");
    }

    private ISurface CreatePixel(Color4 color, string meaning)
    {
        var image = new DirectX.Resources.Surfaces.ImageInfo(1, 1, Format.R16G16B16A16_Float, 1 * Format.R16G16B16A16_Float.BytesPerPixel());
        var mipMap = DirectX.Resources.Surfaces.MipMapInfo.None();
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