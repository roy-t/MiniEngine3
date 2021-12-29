using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ModelVertex
{
    public Vector3 Position;
    public Vector2 Texcoord;
    public Vector3 Normal;

    public ModelVertex(Vector3 position, Vector2 texcoord, Vector3 normal)
    {
        this.Position = position;
        this.Texcoord = texcoord;
        this.Normal = normal;
    }

    public static readonly InputElementDescription[] Elements = new InputElementDescription[]
    {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 3 * sizeof(float), 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 5 * sizeof(float), 0, InputClassification.PerVertexData, 0)
    };
}
