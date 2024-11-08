﻿using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Lines;

public struct LineComponent : IComponent
{
    public ILifetime<LineMesh> Mesh;
    public Color4 Color;
}
