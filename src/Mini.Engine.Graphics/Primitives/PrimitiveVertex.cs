using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.Primitives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PrimitiveVertex
{
    public Vector3 Position;    
    public Vector3 Normal;

    public PrimitiveVertex(Vector3 position, Vector3 normal)
    {
        this.Position = position;        
        this.Normal = normal;
    }

    public static readonly InputElementDescription[] Elements = new InputElementDescription[]
    {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),            
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 3 * sizeof(float), 0, InputClassification.PerVertexData, 0)
    };

    public override readonly string ToString()
    {
        return $"P: {this.Position}, N: {this.Normal}";
    }
}
