using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics.World;

public sealed class GeneratedTerrain
{
    public GeneratedTerrain(RWTexture2D height, RWTexture2D normals, RWTexture2D tint, Mesh mesh)
    {
        this.Height = height;
        this.Normals = normals;
        this.Tint = tint;
        this.Mesh = mesh;
    }

    public RWTexture2D Height { get; }
    public RWTexture2D Normals { get; }
    public RWTexture2D Tint { get; }
    public Mesh Mesh { get; }
}