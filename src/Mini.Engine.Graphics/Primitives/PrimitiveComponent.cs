﻿using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS.Components;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Primitives;

public struct PrimitiveComponent : IComponent
{
    public ILifetime<PrimitiveMesh> Mesh;
}
