using Mini.Engine.DirectX.Buffers;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Graphics;

public interface ITerrain
{
    public IndexBuffer<int> Indices { get; }
    public int TileIndexOffset { get; }
    public int TileIndexCount { get; }

    public VertexBuffer<TerrainVertex> Vertices { get; }
    public StructuredBuffer<Triangle> TrianglesBuffer { get; }
    public ShaderResourceView<Triangle> TrianglesView { get; }
}
