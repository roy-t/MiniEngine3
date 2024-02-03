using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Graphics;
public record class TerrainMesh(List<TerrainVertex> Vertices, List<int> Indices, List<Triangle> Triangles);
