using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Tiles;
public struct TileHighlightComponent : IComponent
{    
    public uint MinColumn;
    public uint MaxColumn;
    public uint MinRow;
    public uint MaxRow;
    public Color4 Tint;
}
