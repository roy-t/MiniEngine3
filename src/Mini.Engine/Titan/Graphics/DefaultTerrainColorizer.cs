using LibGame.Graphics;
using LibGame.Mathematics;

namespace Mini.Engine.Titan.Graphics;
public sealed class DefaultTerrainColorizer : ITerrainColorizer
{
    private readonly ColorPalette ColorPalette;
    private readonly int MinHeight;
    private readonly int MaxHeight;

    public DefaultTerrainColorizer(ColorPalette colorPalette, int minHeight, int maxHeight)
    {
        this.ColorPalette = colorPalette;
        this.MinHeight = minHeight;
        this.MaxHeight = maxHeight;
    }

    public ColorLinear GetColor(IReadOnlyList<Tile> tiles, int i, IReadOnlyList<TerrainVertex> vertices, int a, int b, int c)
    {
        var ya = vertices[a].Position.Y;
        var yb = vertices[b].Position.Y;
        var yc = vertices[c].Position.Y;

        var heigth = Math.Max(ya, Math.Max(yb, yc));
        var paletteIndex = (int)Ranges.Map(heigth, (this.MinHeight, this.MaxHeight), (0.0f, this.ColorPalette.Colors.Count - 1));
        return Colors.RGBToLinear(this.ColorPalette.Colors[paletteIndex]);
    }
}
