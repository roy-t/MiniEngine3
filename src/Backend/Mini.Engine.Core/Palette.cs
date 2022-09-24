using System.Drawing;

namespace Mini.Engine.Core;
public sealed class Palette
{
    private readonly ColorRGB[] ColorList;
    private readonly Random Random;

    public Palette(params ColorRGB[] colors)
    {
        this.ColorList = colors;
        this.Random = new Random();
    }

    public IReadOnlyList<ColorRGB> Colors => this.ColorList;

    public ColorRGB Pick()
    {
        var index = this.Random.Next(this.ColorList.Length);
        return this.ColorList[index];
    }

    private static ColorRGB FromHex(string hex)
    {
        var c = ColorTranslator.FromHtml(hex);
        return new ColorRGB(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
    }

    // Inspired by https://colorpalette.org/grass-green-lawn-color-palette/
    public static Palette GrassLawn { get; } =
        new Palette
        (
            FromHex("#c1dbbc"),
            FromHex("#558e1e"),
            FromHex("#b6d6a8"),
            FromHex("#2a5126"),

            FromHex("#98c680"),
            FromHex("#9dc59c"),
            FromHex("#3d593c"),
            FromHex("#467346"),

            FromHex("#619c3d"),
            FromHex("#83b287"),
            FromHex("#67a059"),
            FromHex("#85b870")
        );

    // Inspired by https://colorpalette.org/green-grass-water-color-palette-2/
    public static Palette GrassWater { get; } =
    new Palette
    (
        FromHex("#94925c"),
        //FromHex("#c8cac0"),
        FromHex("#b2a070"),
        FromHex("#837d3e"),

        FromHex("#2a2717"),
        FromHex("#6e7630"),
        FromHex("#67682b"),
        FromHex("#2b341a"),

        FromHex("#36391b"),
        //FromHex("#687a63"),
        FromHex("#586625")
        //FromHex("#8ba49d")
    );
}
