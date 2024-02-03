using LibGame.Graphics;

namespace Mini.Engine.Titan.Graphics;
public interface ITerrainColorizer
{
    ColorLinear GetColor(IReadOnlyList<Tile> tiles, int i, IReadOnlyList<TerrainVertex> vertices, int a, int b, int c);
}