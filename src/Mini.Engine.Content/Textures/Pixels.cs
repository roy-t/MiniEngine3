using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
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

    public ILifetime<ITexture> WhitePixel => this.CreatePixel(Colors.White, "White");

    public ILifetime<ITexture> BlackPixel => this.CreatePixel(Colors.Black, "Black");

    public ILifetime<ITexture> RedPixel => this.CreatePixel(Colors.Red, "Red");

    public ILifetime<ITexture> GreenPixel => this.CreatePixel(Colors.Green, "Green");

    public ILifetime<ITexture> BluePixel => this.CreatePixel(Colors.Blue, "Blue");

    public ILifetime<ITexture> DefaultMaterialPixel => this.MaterialPixel(0.0f, 0.0f, 1.0f);

    //public ILifetime<ITexture> ConductivePixel => this.CreatePixel(Colors.White, "Metalicness");

    //public ILifetime<ITexture> DielectricPixel => this.CreatePixel(Colors.Black, "Metalicness");

    //public ILifetime<ITexture> RoughPixel => this.RoughnessPixel(1.0f);

    //public ILifetime<ITexture> SmoothPixel => this.RoughnessPixel(0.0f);

    //public ILifetime<ITexture> VisiblePixel => this.AmbientOcclussionPixel(1.0f);

    //public ILifetime<ITexture> OccludedPixel => this.AmbientOcclussionPixel(0.0f);

    public ILifetime<ITexture> AlbedoPixel(Color4 color)
    {
        return this.CreatePixel(color, "Albedo");
    }

    public ILifetime<ITexture> NormalPixel()
    {
        return this.NormalPixel(Vector3.UnitZ);
    }

    public ILifetime<ITexture> NormalPixel(Vector3 direction)
    {
        return this.CreatePixel(new Color4(Pack(direction), 1.0f), "Normal");
    }

    public ILifetime<ITexture> MaterialPixel(float metalicness, float roughness, float ao = 1.0f)
    {
        return this.CreatePixel(new Color4(metalicness, roughness, ao), "Material");
    }   

    public ILifetime<ITexture> CreatePixel(Color4 color, string meaning)
    {
        var image = new ImageInfo(1, 1, Format.R32G32B32A32_Float, 1 * Format.R32G32B32A32_Float.BytesPerPixel());
        var mipMap = MipMapInfo.None();
        var pixel = new Texture(this.Device, nameof(Pixels) + meaning, image, mipMap);
        pixel.SetPixels(this.Device, new ReadOnlySpan<Color4>(new Color4[] { color }));

        return this.Device.Resources.Add(pixel);        
    }

    private static Vector3 Pack(Vector3 direction)
    {
        return 0.5f * (Vector3.Normalize(direction) + Vector3.One);
    }
}