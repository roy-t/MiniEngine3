using Mini.Engine.ECS.Components;
using Mini.Engine.ECS;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Tiles;
public struct TileHighlightComponent : IComponent
{    
    public uint StartColumn;
    public uint EndColumn;
    public uint StartRow;
    public uint EndRow;
    public Color4 Tint;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
