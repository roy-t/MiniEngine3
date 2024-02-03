namespace Mini.Engine.Titan.Graphics;

public interface ITerrainBuilder
{
    TerrainMesh Build(Tile[] tiles, ITerrainColorizer colorizer, int columns, int rows);
}