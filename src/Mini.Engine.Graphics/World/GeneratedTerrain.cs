using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Graphics.World;

public sealed class GeneratedTerrain
{
    public GeneratedTerrain(IRWTexture height, IRWTexture normals, IRWTexture erosion, Mesh mesh)
    {
        this.Height = height;
        this.Normals = normals;
        this.Erosion = erosion;
        this.Mesh = mesh;
    }

    public IRWTexture Height { get; }
    public IRWTexture Normals { get; }
    public IRWTexture Erosion { get; }
    public Mesh Mesh { get; }
}