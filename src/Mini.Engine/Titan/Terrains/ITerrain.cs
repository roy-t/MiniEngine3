using Mini.Engine.DirectX.Buffers;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Terrains;

public interface ITerrain
{
    public IndexBuffer<int> Indices { get; }
    public int TileIndexOffset { get; }
    public int TileIndexCount { get; }

    public VertexBuffer<TerrainVertex> Vertices { get; }
    public StructuredBuffer<Triangle> TrianglesBuffer { get; }
    public ShaderResourceView<Triangle> TrianglesView { get; }

    public IReadOnlyList<Tile> Tiles { get; }
}
