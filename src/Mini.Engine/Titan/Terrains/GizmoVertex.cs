using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;

namespace Mini.Engine.Titan.Terrains;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct GizmoVertex(Vector3 position)
{
    public readonly Vector3 Position = position;
    public static readonly InputElementDescription[] Elements =
    [
        new("POSITION", 0, Vortice.DXGI.Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
    ];

    public override readonly string ToString()
    {
        return this.Position.ToString();
    }
}
