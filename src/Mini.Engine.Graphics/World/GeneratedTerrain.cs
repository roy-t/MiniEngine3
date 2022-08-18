using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.vNext;

namespace Mini.Engine.Graphics.World;

public sealed class GeneratedTerrain
{
    public GeneratedTerrain(IRWTexture height, IRWTexture normals, IRWTexture tint, Mesh mesh)
    {
        this.Height = height;
        this.Normals = normals;
        this.Tint = tint;
        this.Mesh = mesh;
    }

    public IRWTexture Height { get; }
    public IRWTexture Normals { get; }
    public IRWTexture Tint { get; }
    public Mesh Mesh { get; }
}