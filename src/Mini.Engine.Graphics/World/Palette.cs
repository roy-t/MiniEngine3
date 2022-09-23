using System.Drawing;
using System.Numerics;

namespace Mini.Engine.Graphics.World;
public sealed class Palette
{
    private readonly Vector3[] ColorList;
    private readonly Random Random;

    public Palette(params Vector3[] colors)
    {
        this.ColorList = colors;
        this.Random = new Random();
    }

    public IReadOnlyList<Vector3> Colors => this.ColorList;

    public Vector3 Pick()
    {
        var index = this.Random.Next(this.ColorList.Length);
        return this.ColorList[index];
    }


    public static Palette Grass()
    {        
        // https://i2.wp.com/colorpalette.org/wp-content/palette/grass_green_lawn_colorpalette_r8wox.jpg?q=100
        return new Palette
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
    }


    private static Vector3 FromHex(string hex)
    {
        var c = ColorTranslator.FromHtml(hex);
        return new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
    }
}
